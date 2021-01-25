from uuid import uuid4
from luna.utils import ProjectUtils
from Agent import key_vault_client
import json
import tempfile
import zipfile
import os
import requests
from datetime import date, datetime
from Agent.Exception.LunaExceptions import LunaServerException, LunaUserException
from http import HTTPStatus
from adal import AuthenticationContext
import base64
from Agent.Data.GitRepo import GitRepo
import mlflow
from mlflow.tracking import MlflowClient

AAD_RESOURCE_URL_FORMAT = "https://login.microsoftonline.com/{}"
MGMT_TOKEN_RESOURCE_ID = "https://management.core.windows.net/"
ACCESS_TOKEN_RESOURCE_ID = "2ff814a6-3304-4ab8-85cb-cd0e6f879c1d"
DATETIME_FORMAT = "%Y-%m-%d %H:%M:%S.%f"
ADB_REST_URL_FORMAT = "{}/api/2.0/{}"

class AzureDatabricksUtils(object):
    _access_token = ""
    _mgmt_token = ""
    _token_expires_on = datetime.max
    _workspace = None

    def __init__(self, workspace):
        if workspace.AADApplicationSecret:
            workspace.AADApplicationSecret =key_vault_client.get_secret(workspace.AADApplicationSecretName).value
        self._workspace = workspace
        self.refreshTokens()

    def refreshTokens(self):
        auth_context = AuthenticationContext(AAD_RESOURCE_URL_FORMAT.format(self._workspace.AADTenantId))

        token_response = auth_context.acquire_token_with_client_credentials(MGMT_TOKEN_RESOURCE_ID, self._workspace.AADApplicationId, self._workspace.AADApplicationSecret)
        self._mgmt_token = token_response["accessToken"]
        expires_on = datetime.strptime(token_response["expiresOn"], DATETIME_FORMAT)
        if expires_on < self._token_expires_on:
            self._token_expires_on = expires_on

        token_response = auth_context.acquire_token_with_client_credentials(ACCESS_TOKEN_RESOURCE_ID, self._workspace.AADApplicationId, self._workspace.AADApplicationSecret)
        self._access_token = token_response["accessToken"]
        expires_on = datetime.strptime(token_response["expiresOn"], DATETIME_FORMAT)
        if expires_on < self._token_expires_on:
            self._token_expires_on = expires_on

    def getMgmtToken(self):
        if self._token_expires_on < datetime.utcnow():
            self.refreshTokens()
        return self._mgmt_token

    def getAccessToken(self):
        if self._token_expires_on < datetime.utcnow():
            self.refreshTokens()
        return self._access_token

    def send_get_request(self, url, body):
        headers = {}
        headers["X-Databricks-Azure-Workspace-Resource-Id"] = self._workspace.ResourceId
        headers["X-Databricks-Azure-SP-Management-Token"] = self.getMgmtToken()
        headers["Authorization"] = "Bearer {}".format(self.getAccessToken())
        response = requests.get(url, body, headers=headers)
        return response.json()

    def getModel(self, mlModel):
        url = ADB_REST_URL_FORMAT.format(self._workspace.WorkspaceUrl, "mlflow/registered-models/get")
        
        body = {
            "name": mlModel.ModelName,
            "version": mlModel.ModelVersion
            }

        return self.send_get_request(url, body)

    def getExperimentName(self, subscriptionId):
        return "/Users/{}/{}".format(self._workspace.AADApplicationId.lower(), subscriptionId)

    def runProject(self, subscription, apiVersion, operationName, userInput, predecessorOperationId='na'):
        os.environ['MLFLOW_TRACKING_URI'] = 'databricks'
        os.environ['DATABRICKS_HOST'] = self._workspace.WorkspaceUrl
        os.environ['DATABRICKS_TOKEN'] = self.getAccessToken()

        # TODO: use the real config file
        backend_config = {
            "spark_version": "7.3.x-scala2.12",
            "num_workers": 1,
            "node_type_id": "Standard_DS3_v2"
        }
        
        #with open('backend_config.json', 'w+') as file:
        #    json.dump(backend_config, file)
        mlflow.set_tracking_uri("databricks")
        mlflow.set_registry_uri("databricks")
        # mlflow.set_tracking_uri(os.environ['ODBC_CONNECTION_STRING'])
        exp_name = self.getExperimentName(subscription.SubscriptionId)
        exp = mlflow.get_experiment_by_name(exp_name)

        if not exp:
            mlflow.create_experiment(exp_name)
            
        mlflow.set_experiment(exp_name)
        with mlflow.start_run():
            repo = GitRepo.GetById(apiVersion.GitRepoId)
            fullUrl = "https://{}@{}".format(repo.PersonalAccessToken, repo.HttpUrl[8:])
            mlflow.run(fullUrl, 
                     parameters = userInput,
                     entry_point = operationName, 
                     experiment_name=exp_name, 
                     backend="databricks", 
                     backend_config = backend_config,
                     version = apiVersion.GitVersion,
                     synchronous = False)
        
            operationId = str('a' + uuid4().hex[1:])
            tags={'userId': subscription.Owner, 
              'aiServiceName': subscription.AIServiceName, 
              'aiServicePlanName': subscription.AIServicePlanName, 
              'apiVersion': apiVersion.VersionName,
              'operationName': operationName,
              'operationId': operationId,
              'subscriptionId': subscription.SubscriptionId,
              'predecessorOperationId': predecessorOperationId}
            mlflow.set_tags(tags)

        return operationId
    
    def getOperationOutput(self, operationId, userId, subscriptionId, outputType = "json"):
        
        runInfo, operationName = self.getRunInfoByTags(operationId, userId, subscriptionId)
        client = MlflowClient()

        if outputType == "json":
            try:
                with tempfile.TemporaryDirectory() as tmp:
                    path = os.path.join(tmp, 'output/output.json')
                    files = client.download_artifacts(runInfo['run_id'], 'output/output.json', tmp)
                    with open(path) as file:
                        return json.load(file)
            except Exception as ex:
                raise LunaUserException(HTTPStatus.NOT_FOUND, "JSON output of operation {} does not exist or you do not have permission to access it.".format(operationId))
        elif outputType == "file":
            tmp = tempfile.TemporaryDirectory().name
            localPath = os.path.join(tmp, "output")
            if not os.path.exists(localPath):
                os.makedirs(localPath)
            local_path = client.download_artifacts(runInfo['run_id'], "output", localPath)
        
            zip_file_path = os.path.join(tmp, "output_{}.zip".format(operationId))
            zipf = zipfile.ZipFile(zip_file_path, "w", zipfile.ZIP_DEFLATED)
            self.zipdir(localPath, zipf, "output_{}".format(operationId))
            zipf.close()
            return zip_file_path

    def getRunInfoByTags(self, operationId, userId, subscriptionId):
               
        os.environ['MLFLOW_TRACKING_URI'] = 'databricks'
        os.environ['DATABRICKS_HOST'] = self._workspace.WorkspaceUrl
        os.environ['DATABRICKS_TOKEN'] = self.getAccessToken()
        
        mlflow.set_registry_uri("databricks")
        
        exp_name = self.getExperimentName(subscriptionId)
        exp = mlflow.get_experiment_by_name(exp_name)
        filter_string = "tags.userId ILIKE '{}' AND tags.operationId ILIKE '{}' AND tags.subscriptionId ILIKE '{}'".format(userId, operationId, subscriptionId)
        runs = mlflow.search_runs([exp.experiment_id], filter_string=filter_string)

        if runs.shape[0] == 0:
            raise LunaUserException(HTTPStatus.NOT_FOUND, "The operation {} does not exist or you do not have permission to acces it.".format(operationId))

        operationName = runs.iloc[0]['tags.operationName']
        filter_string = "tags.mlflow.parentRunId ILIKE '{}'".format(runs.iloc[0]['run_id'])
        
        runs = mlflow.search_runs([exp.experiment_id], filter_string=filter_string)

        if runs.shape[0] == 0:
            raise LunaUserException(HTTPStatus.NOT_FOUND, "The operation {} does not exist or you do not have permission to acces it.".format(operationId))

        return runs.iloc[0], operationName

    def listAllOperations(self, operationName, userId, subscriptionId):
        
        os.environ['MLFLOW_TRACKING_URI'] = 'databricks'
        os.environ['DATABRICKS_HOST'] = self._workspace.WorkspaceUrl
        os.environ['DATABRICKS_TOKEN'] = self.getAccessToken()
        
        mlflow.set_registry_uri("databricks")
        
        exp_name = self.getExperimentName(subscriptionId)
        exp = mlflow.get_experiment_by_name(exp_name)
        filter_string = "tags.userId ILIKE '{}' AND tags.operationName ILIKE '{}' AND tags.subscriptionId ILIKE '{}'".format(userId, operationName, subscriptionId)
        runs = mlflow.search_runs([exp.experiment_id], filter_string=filter_string)

        resultList = []
        for index, row in runs.iterrows():
            
            filter_string = "tags.mlflow.parentRunId ILIKE '{}'".format(row['run_id'])
            child_runs = mlflow.search_runs([exp.experiment_id], filter_string=filter_string)
            result = {'operationId': row["tags.operationId"],
                      'operationName': operationName,
                      'startTime': child_runs.iloc[0]['start_time'],
                      'endTime': child_runs.iloc[0]['end_time'],
                      'status': child_runs.iloc[0]['status']
                }
            resultList.append(result)

        return resultList

    def getOperationStatus(self, operationId, userId, subscriptionId):
 
        run, operationName = self.getRunInfoByTags(operationId, userId, subscriptionId)
        result = {'operationId': operationId,
                  'operationName': operationName,
                  'startTime': run['start_time'],
                  'endTime': run['end_time'],
                  'status': run['status']
                }

        return result

    def downloadModel(self, mlModel):
        # TODO: see if we can use python library
        body = {
            "name": mlModel.ModelName,
            "version": mlModel.ModelVersion
            }

        response = self.send_get_request(url, body)
        artifacts_path = response["artifact_uri"][5:]
        
        tmp = tempfile.TemporaryDirectory().name
        localPath = os.path.join(tmp, "model")
        self.downloadFiles(localPath, artifacts_path, True, 0)
        
        zip_file_path = os.path.join(tmp, "model_{}.zip".format(mlModel.ModelName))
        zipf = zipfile.ZipFile(zip_file_path, "w", zipfile.ZIP_DEFLATED)
        self.zipdir(localPath, zipf, "model_{}".format(mlModel.ModelName))
        zipf.close()
        return zip_file_path
        
    def zipdir(self, path, ziph, dir):
    # ziph is zipfile handle
        for root, dirs, files in os.walk(path):
            for file in files:
                ziph.write(os.path.join(root, file), os.path.relpath(os.path.join(root, file), path))

    def downloadFile(self, localPath, path, size):
        with open(localPath, 'wb+') as file:
            while True:
                offset = 0
                url = ADB_REST_URL_FORMAT.format(self._workspace.WorkspaceUrl, "dbfs/read")
                body = {
                    "path": path,
                    "offset": offset,
                    "length": 1024 * 1024
                    }
                response = self.send_get_request(url, body)
                file.write(base64.b64decode(response["data"]))
                offset = offset + 1024 * 1024
                if offset > size:
                    break;
        return

    def getFileNameFromPath(self, path):
        return path[path.rindex('/')+1:]

    def downloadFiles(self, localPath, path, is_dir, size):
        
        # download the file
        if not is_dir:
            self.downloadFile(localPath, path, size)
        else:
            if not os.path.exists(localPath):
                os.makedirs(localPath)
            url = ADB_REST_URL_FORMAT.format(self._workspace.WorkspaceUrl, "dbfs/list")

            body = {
                "path": path
                }

            files = self.send_get_request(url, body)
            for file in files["files"]:
                self.downloadFiles(os.path.join(localPath, self.getFileNameFromPath(file["path"])), file["path"], file["is_dir"], file["file_size"])

