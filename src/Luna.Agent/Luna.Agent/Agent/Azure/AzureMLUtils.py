from uuid import uuid4
from azureml.core import Workspace, Experiment, Model
from azureml.core.webservice import AksWebservice, Webservice, AciWebservice
from azureml.pipeline.core import PublishedPipeline
from azureml.core.authentication import ServicePrincipalAuthentication
from luna.utils import ProjectUtils
from Agent import key_vault_client
import json
import tempfile
import zipfile
import os
from Agent.Exception.LunaExceptions import LunaServerException, LunaUserException
from Agent.Data.AMLPipelineEndpoint import AMLPipelineEndpoint
from http import HTTPStatus
import mlflow
import mlflow.azureml
from Agent.Data.GitRepo import GitRepo
from mlflow.exceptions import ExecutionException

class AzureMLUtils(object):
    """The utlitiy class to execute and monitor runs in AML"""
    _utils=None
    
    def GetOperationNameByVerb(self, verb):
        if verb == 'deploy' or verb == 'batchinference' or verb == 'train':
            return verb
        return self._utils.GetOperationNameByVerb(verb)

    def GetOperationNameByNoun(self, noun):
        if noun == 'models':
            return 'train'
        if noun == 'inferenceresult':
            return 'batchinference'
        if noun == 'endpoints':
            return 'deploy'

        return self._utils.GetOperationNameByNoun(noun)

    def get_workspace_info_from_resource_id(self, resource_id):
        infoList = resource_id.split('/')
        subscriptionId = infoList[2]
        resourceGroupName = infoList[4]
        workspaceName = infoList[-1]
        return subscriptionId, resourceGroupName, workspaceName

    def __init__(self, workspace):
        if workspace.AADApplicationSecret:
            secret = workspace.AADApplicationSecret
        else:
            secret = key_vault_client.get_secret(workspace.AADApplicationSecretName).value
        auth = ServicePrincipalAuthentication(
            tenant_id = workspace.AADTenantId,
            service_principal_id = workspace.AADApplicationId,
            service_principal_password = secret)
        subscriptionId, resourceGroupName, workspaceName = self.get_workspace_info_from_resource_id(workspace.ResourceId)
        ws = Workspace(subscriptionId, resourceGroupName, workspaceName, auth)
        self._workspace = ws

    def downloadModel(self, mlModel):
        if mlModel.ModelVersion == 0:
            mlModel.ModelVersion = None
        model = Model(self._workspace, name=mlModel.ModelName, version = mlModel.ModelVersion)

        if not model:
            raise LunaUserException(HTTPStatus.NOT_FOUND, "Model not found in the model repo. Contact the publisher to correct the error.");
        
        tmp = tempfile.TemporaryDirectory().name
        path = os.path.join(tmp, "model")
        zip_file_path = os.path.join(tmp, "model_{}.zip".format(mlModel.ModelName))
        files = model.download(path)
        zipf = zipfile.ZipFile(zip_file_path, "w", zipfile.ZIP_DEFLATED)
        self.zipdir(path, zipf, "model_{}".format(mlModel.ModelName))
        zipf.close()
        return zip_file_path

    def getEndpoint(self, apiVersion):
        service = Webservice(self._workspace, apiVersion.EndpointName)
        
        if not service:
            raise LunaUserException(HTTPStatus.NOT_FOUND, "Service endpoint not found in the AML workspace. Contact the publisher to correct the error.");
        return service

    def get_pipeline_id_from_url(self, url):
        list = url.split('/')
        return list[-1]

    def submitPipelineRun(self, subscription, apiVersion, pipelineEndpoint, userInput, predecessorOperationId = 'na'):
        operationId = str('a' + uuid4().hex[1:])
        experimentName = subscription.SubscriptionId
        exp = Experiment(self._workspace, experimentName)
        tags={'userId': subscription.Owner, 
              'aiServiceName': subscription.AIServiceName, 
              'aiServicePlanName': subscription.AIServicePlanName, 
              'apiVersion': apiVersion.VersionName,
              'operationName': pipelineEndpoint.PipelineEndpointName,
              'operationId': operationId,
              'subscriptionId': subscription.SubscriptionId,
              'predecessorOperationId': predecessorOperationId}
        pipeline = PublishedPipeline.get(workspace = self._workspace, id = pipelineEndpoint.PipelineEndpointId.lower())
        exp.submit(pipeline, tags = tags, pipeline_parameters=userInput)
        return operationId
     
    def runProject(self, subscription, apiVersion, operationName, userInput, predecessorOperationId='na'):
        
        operationId = str('a' + uuid4().hex[1:])
        experimentName = subscription.SubscriptionId
        tags={'userId': subscription.Owner, 
              'aiServiceName': subscription.AIServiceName, 
              'aiServicePlanName': subscription.AIServicePlanName, 
              'apiVersion': apiVersion.VersionName,
              'operationName': operationName,
              'operationId': operationId,
              'subscriptionId': subscription.SubscriptionId,
              'predecessorOperationId': predecessorOperationId}
        
        mlflow.set_tracking_uri(self._workspace.get_mlflow_tracking_uri())
        mlflow.set_experiment(experimentName)
        backend_config = {"COMPUTE": apiVersion.LinkedServiceComputeTarget, "USE_CONDA": True}
        
        repo = GitRepo.GetById(apiVersion.GitRepoId)
        fullUrl = "https://{}@{}".format(repo.PersonalAccessToken, repo.HttpUrl[8:])
        # work around a logging issue in AML to avoid logging PAT
        os.environ['AZUREML_GIT_REPOSITORY_URI'] = repo.HttpUrl
        try:
            run = mlflow.projects.run(uri=fullUrl, 
                                  version = apiVersion.GitVersion,
                                  entry_point= operationName,
                                  parameters=userInput,
                                  backend = "azureml",
                                  backend_config = backend_config,
                                  synchronous=False)
        except ExecutionException as e:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, str(e.message))
        run._run.set_tags(tags)
        return operationId

    def getOperationStatus(self, operationId, userId, subscriptionId, runType = "azureml.PipelineRun"):
        experimentName = subscriptionId
        exp = Experiment(self._workspace, experimentName)
        tags = {'userId': userId,
                'operationId': operationId,
                'subscriptionId': subscriptionId}
        runs = exp.get_runs(type=runType, tags=tags)
        try:
            run = next(runs)
            details = run.get_details()
            result = {'operationId': operationId,
                      'operationName': run.tags["operationName"],
                      'startTime': details["startTimeUtc"],
                      'endTime': details["endTimeUtc"],
                      'status': run.status
                }
            return result
        except StopIteration:
            raise LunaUserException(HTTPStatus.NOT_FOUND, 'Operation with id {} does not exist.'.format(operationId))

    def listAllOperations(self, operationName, userId, subscriptionId, runType = "azureml.PipelineRun"):
        experimentName = subscriptionId
        exp = Experiment(self._workspace, experimentName)
        tags = {'userId': userId,
                'operationName': operationName,
                'subscriptionId': subscriptionId}
        runs = exp.get_runs(type=runType, tags=tags)
        resultList = []
        while True:
            try:
                run = next(runs)
                details = run.get_details()
                result = {'operationId': run.tags["operationId"],
                          'operationName': operationName,
                          'startTime': details["startTimeUtc"],
                          'endTime': details["endTimeUtc"],
                          'status': run.status
                    }
                resultList.append(result)
            except StopIteration:
                break
        return resultList

    def getOperationOutput(self, operationName, operationId, userId, subscriptionId, runType="azureml.PipelineRun", outputType = "json"):
        
        tags = {'userId': userId,
                'operationId': operationId,
                'subscriptionId': subscriptionId}

        experimentName = subscriptionId
        exp = Experiment(self._workspace, experimentName)
        runs = exp.get_runs(type=runType, tags=tags)
        try:
            run = next(runs)
            child_runs = run.get_children()
            child_run = next(child_runs)
            if outputType == 'json':
                with tempfile.TemporaryDirectory() as tmp:
                    path = os.path.join(tmp, 'output.json')
                    files = child_run.download_file('/outputs/output.json', path)
                    with open(path) as file:
                        return json.load(file), "json"
            elif outputType == 'file':
                tmp = tempfile.TemporaryDirectory().name
                path = os.path.join(tmp, "outputs")
                zip_file_path = os.path.join(tmp, "output_{}.zip".format(operationId))
                files = child_run.download_files("/outputs", path, append_prefix=False)
                zipf = zipfile.ZipFile(zip_file_path, "w", zipfile.ZIP_DEFLATED)
                self.zipdir(path, zipf, "outputs")
                zipf.close()
                return zip_file_path, "file"
            else:
                return None, None
        except StopIteration:
            return None, None

    def zipdir(self, path, ziph, dir):
    # ziph is zipfile handle
        for root, dirs, files in os.walk(path):
            for file in files:
                ziph.write(os.path.join(root, file), arcname = os.path.relpath(os.path.join(root, file), path))

    def listAllOperationOutputs(self, operationNoun, userId, subscriptionId):
        operationName = self.GetOperationNameByNoun(operationNoun)
        experimentName = subscriptionId
        exp = Experiment(self._workspace, experimentName)
        tags = {'userId': userId,
                'operationName': operationName,
                'subscriptionId': subscriptionId}
        runs = exp.get_runs(type='azureml.PipelineRun', tags=tags)
        results = []
        while True:
            try:
                run = next(runs)
                output, outputType = self.getOperationOutput(operationNoun, run.tags["operationId"], userId, subscriptionId, downloadFiles=False)
                if output:
                    if outputType == "model" or outputType == "endpoint":
                        results.append(output)
                    elif outputType == "json":
                        results.append({"operationId": run.tags["operationId"], "output": result})
                    elif outputType == "file":
                        results.append({"operationId": run.tags["operationId"], "outputType": "file"})
            except StopIteration:
                break
        return results

    def deleteOperationOutput(self, productName, deploymentName, apiVersion, operationName, operationId, userId, subscriptionId):
        return

    def getComputeClusters(self):
        clusters = self._workspace.compute_targets
        computeClusters = []
        for cluster in clusters.values():
            if cluster.type == "AmlCompute":
                computeClusters.append(cluster.name)
        return computeClusters

    def getDeploymentClusters(self):
        clusters = self._workspace.compute_targets
        deploymentClusters = []
        for cluster in clusters.values():
            if cluster.type == "AKS":
                deploymentClusters.append(cluster.name)
        return deploymentClusters