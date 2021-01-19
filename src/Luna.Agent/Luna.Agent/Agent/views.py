"""
Routes and views for the flask application.
"""

from datetime import datetime
from flask import render_template, send_file,redirect
from flask import jsonify, request
from Agent.Code.CodeUtils import CodeUtils
from Agent.Azure.AzureMLUtils import AzureMLUtils
from Agent.Azure.AzureDatabricksUtils import AzureDatabricksUtils
from Agent.Mgmt.ControlPlane import ControlPlane
from datetime import datetime
from uuid import uuid4
import pathlib
from Agent.Data.AzureDatabricksWorkspace import AzureDatabricksWorkspace
from Agent.Data.AMLPipelineEndpoint import AMLPipelineEndpoint
from Agent.Data.APISubscription import APISubscription
from Agent.Data.Subscription import Subscription
from Agent.Data.APIVersion import APIVersion
from Agent.Data.MLModel import MLModel
from sqlalchemy.orm import sessionmaker
from Agent import engine, Session, app, key_vault_client
from azure.keyvault.secrets import SecretClient
from azure.identity import DefaultAzureCredential
from Agent.Data.AMLWorkspace import AMLWorkspace
from Agent.Data.AgentUser import AgentUser
from Agent.Data.Publisher import Publisher
from Agent.Data.Offer import Offer
from Agent.Exception.LunaExceptions import LunaServerException, LunaUserException
from Agent.Auth.AuthHelper import AuthenticationHelper
from Agent.Data.GitRepo import GitRepo
from Agent.Azure.GitUtils import GitUtils
import json, os, io
from http import HTTPStatus
import requests
from cryptography import x509
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives import hashes
import binascii

def getToken():
    bearerToken = request.headers.get("Authorization")
    if not bearerToken or not bearerToken.startswith("Bearer "):
        raise LunaUserException(HTTPStatus.FORBIDDEN, "AAD token is required.")
    return bearerToken.replace("Bearer ", "")

def handleExceptions(e):
    if isinstance(e, LunaUserException):
        return e.message, e.http_status_code.value
    else:
        app.logger.info(e)
        return 'The server encountered an internal error and was unable to complete your request.', 500

def getAPIVersion(subscription):
    version = request.args.get('api-version')
    if not version:
        raise LunaUserException(HTTPStatus.BAD_REQUEST, 'The api-version query parameter is not provided.')

    apiVersion = APIVersion.Get(subscription.AIServiceName, subscription.AIServicePlanName, version)
    if not apiVersion:
        raise LunaUserException(HTTPStatus.NOT_FOUND, 'The specified api version does not exist or you do not have permission to access it.')

    return apiVersion;

def convertOnelinePemtoPemData(pem):
    
    result = "-----BEGIN CERTIFICATE-----\n"

    index = 0

    while index*64 < len(pem):
        if (index + 1) * 64 < len(pem):
            result = "{}{}\n".format(result, pem[index*64:index*64+64])
        else:
            result = "{}{}\n".format(result, pem[index*64:])
        index = index + 1

    result = "{}-----END CERTIFICATE-----\n".format(result)

    return str.encode(result)

def validateAPIKeyAndGetSubscription(subscriptionId, serviceName = None, planName = None):
    
    pem = request.headers.get('X-ARR-ClientCert')
    if not pem:
        pem = "MIIDEjCCAfqgAwIBAgIQd8JlPHPUz41AD9vEd2UhhjANBgkqhkiG9w0BAQsFADASMRAwDgYDVQQDDAdsdW5hLmFpMB4XDTIxMDExNTE2MzAxOVoXDTIyMDExNTE2NTAxOVowEjEQMA4GA1UEAwwHbHVuYS5haTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKXi+qHK6YISiGrGbXgXC4+tm37vR5vIS+L6q5o7VK2vAYrR4axcWak3pPWtgr/MG46bhole0Lz92kZA2H4Pc6krZNFYfhjTkm/KZ4LN8YYPTy25ZCRX51QXl9wvbqoZ5ZwJxr5n44kQ1e+3Ay5MNdO5zKcjFmAUfy1cDhpB37hFh5R1nGL1ePw45SzmPOqJ6tdP3nUz4edJu8eaHt8WDViA5vS1wPh1JuW+oPqr46ufo9VQ6nGQp6SY6O6+E7LrbOaFOf+JbepcbAf7gZ2sprCA6q1bwFJskWofT1pFouDxFNqWLseaRNM+VI4dweBli+PX5bxc1e/G68gfJSolDpkCAwEAAaNkMGIwDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDATASBgNVHREECzAJggdsdW5hLmFpMB0GA1UdDgQWBBTK4a+0Galc8Sy9fkBxMs3oszECnjANBgkqhkiG9w0BAQsFAAOCAQEAWIfmsu5ZCOXkJS9jLXAK7bmZBjGezanjB/WaKCIwfPUOx2YWHMYLFLgS39QF0ZntBh3QCxtgAxySsjWd7UFszLEOsIhkKHQnPro8BZ2hG3aJZNiiU6MlSA6Zd6ddpVsgZOVtG1K5CdeM5vCLF+qKobMdjWy9FmWO8fcUrWT1wkvf2aBoedqbzQCwIE5Q3mTyIa3maPGLxkrs4lRbpVuKKfPVuEzLgtjjt0V+le7VkZKLuJ6mMKmR3HN3dEE9RJzE2yCWjWKTBnYj1slWmJw+G/TNgezlJE6Fl5/wZ2IeGJOrgkfV+haS9hH3rymAflxvc8PlAcDy9J+yVxpwKRDBeQ=="
    if pem:
        cert = x509.load_pem_x509_certificate(convertOnelinePemtoPemData(pem), default_backend())

        #validate thumbprint
        if binascii.hexlify(cert.fingerprint(hashes.SHA1())).upper() != b"46FE14FDE5F55158FCD472E43A80474802D8A5CB":
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, 'Invalid certificate.')
        if cert.issuer.rfc4514_string() != 'CN=luna.ai':
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, 'Invalid certificate.')
        if cert.subject.rfc4514_string() != 'CN=luna.ai':
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, 'Invalid certificate.')
        if datetime.utcnow() > cert.not_valid_after or datetime.utcnow() < cert.not_valid_before:
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, 'Invalid certificate.')

        sub = Subscription()
        sub.AIServiceName = serviceName
        sub.AIServicePlanName = planName
        subscriptionId = request.headers.get('Luna-Subscription')
        if not subscriptionId:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, 'Subscription id id not available.')
        sub.SubscriptionId = subscriptionId
        user = request.headers.get('Luna-User')
        if not user:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, 'User id id not available.')
        sub.Owner = user
        return sub

    subscriptionKey = request.headers.get('api-key')
    if subscriptionKey:
        sub = Subscription.GetByKey(subscriptionKey)
        if not sub:
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, 'The api key is invalid.')
        if subscriptionId != "default" and subscriptionId.lower() != sub.SubscriptionId.lower():
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, "The subscription {} doesn't exist or api key is invalid.".format(subscriptionId))
    #else:
    #    objectId = AuthenticationHelper.ValidateSignitureAndUser(getToken(), subscriptionId)
    #    sub = Subscription.Get(subscriptionId, objectId)
    #    if not sub:
    #        raise LunaUserException(HTTPStatus.NOT_FOUND, 'The subscription {} does not exist.'.format(subscriptionId))

    return sub

@app.route('/apiv2/models', methods=['GET'])
@app.route('/apiv2/<subscriptionId>/models', methods=['GET'])
def listModels(subscriptionId = 'default'):
    
    try:
        subscription = validateAPIKeyAndGetSubscription(subscriptionId);
        apiVersion = getAPIVersion(subscription);
        if (apiVersion.PlanType != 'model'):
            raise LunaUserException(HTTPStatus.NOT_FOUND, "No model published in the current AI service plan.");
        models = MLModel.ListAll(apiVersion.Id);

        return jsonify(models)
    except Exception as e:
        return handleExceptions(e)
    
@app.route('/apiv2/models/<modelName>', methods=['GET'])
@app.route('/apiv2/<subscriptionId>/models/<modelName>', methods=['GET'])
def getModel(modelName, subscriptionId = 'default'):
    try:
        subscription = validateAPIKeyAndGetSubscription(subscriptionId);
        apiVersion = getAPIVersion(subscription);
        if (apiVersion.PlanType != 'model'):
            raise LunaUserException(HTTPStatus.NOT_FOUND, "No model published in the current AI service plan.");

        mlModel = MLModel.Get(apiVersion.Id, modelName)
        if apiVersion.LinkedServiceType == 'AML':
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            modelZipFilePath = amlUtil.downloadModel(mlModel)
        elif apiVersion.LinkedServiceType == 'ADB':
            adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
            adbUtil = AzureDatabricksUtils(adbWorkspace)
            modelZipFilePath = adbUtil.downloadModel(mlModel)
        else:
            raise LunaUserException(HTTPStatus.NOT_FOUND, "Can not connect to the model repository. Contact the publisher to correct the error.");
        
        with open(modelZipFilePath, 'rb') as bites:
            return send_file(
                 io.BytesIO(bites.read()),
                 attachment_filename='model_{}.zip'.format(modelName),
                 mimetype='application/zip'
            )
    except Exception as e:
        return handleExceptions(e)
    

@app.route('/apiv2/predict', methods=['POST'])
@app.route('/apim/<serviceName>/<planName>/predict', methods=['POST'])
def realtimePredict(subscriptionId = 'default', serviceName = None, planName = None):
    try:
        subscription = validateAPIKeyAndGetSubscription(subscriptionId, serviceName, planName);
        apiVersion = getAPIVersion(subscription);
        if (apiVersion.PlanType != 'endpoint'):
            raise LunaUserException(HTTPStatus.NOT_FOUND, "No service endpoint published in the current AI service plan.")
    
        headers = {'Content-Type': 'application/json'}

        if apiVersion.IsManualInputEndpoint:
            requestUrl = apiVersion.EndpointUrl
            if apiVersion.EndpointAuthType == 'API_KEY':
                secret = key_vault_client.get_secret(apiVersion.EndpointAuthSecretName).value
                if apiVersion.EndpointAuthAddTo == 'HEADER':
                    headers[apiVersion.EndpointAuthKey] = secret
                elif apiVersion.EndpointAuthType == 'QUERY_ARAMETER':
                    requestUrl = "{}?{}={}".format(requestUrl, apiVersion.EndpointAuthKey, secret)
                else:
                    raise LunaServerException("Unknow endpoint auth add-to target.")
            elif apiVersion.EndpointAuthType == 'SERVICE_PRINCIPAL':
                raise LunaUserException(HTTPStatus.NOT_IMPLEMENTED, "Service principal auth is not supported yet.")
        elif apiVersion.LinkedServiceType == 'AML':
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            endpoint = amlUtil.getEndpoint(apiVersion)
            requestUrl = endpoint.scoring_uri
            if endpoint.auth_enabled:
                if endpoint.token_auth_enabled:
                    key, refresh = endpoint.get_token()
                else:
                    key, secondaryKey = endpoint.get_keys()
                headers['Authorization'] = 'Bearer {}'.format(key)
        elif apiVersion.LinkedServiceType == 'ADB':
            adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
            adbUtil = AzureDatabricksUtils(adbWorkspace)
            requestUrl = "{}/model/{}/{}/invocations".format(adbWorkspace.WorkspaceUrl, apiVersion.EndpointName, apiVersion.EndpointVersion)
            headers["Authorization"] = "Bearer {}".format(adbUtil.getAccessToken())

        response = requests.post(requestUrl, json.dumps(request.json), headers=headers)
        if response.ok:
            return response.json(), response.status_code
        return response.text, response.status_code
    except Exception as e:
        return handleExceptions(e)
    
@app.route('/apiv2/operations/metadata', methods=['GET'])
@app.route('/apiv2/<subscriptionId>/operations/metadata', methods=['GET'])
def listPublishedOperations(subscriptionId = 'default'):
    try:
        subscription = validateAPIKeyAndGetSubscription(subscriptionId);
        apiVersion = getAPIVersion(subscription);
        if apiVersion.PlanType == 'pipeline':
            if apiVersion.LinkedServiceType != 'AML':
                raise LunaServerException("No AML workspace found for subscription {}".format(subscriptionId))
            operations = AMLPipelineEndpoint.ListAll(apiVersion.Id)
        elif apiVersion.PlanType == 'mlproject':
            gitUtil = GitUtils(GitRepo.GetById(apiVersion.GitRepoId))
            operations = gitUtil.getEntryPoints(apiVersion.GitVersion)
        else:
            raise LunaUserException(HTTPStatus.NOT_FOUND, "No operation published in the current AI service plan.".format(operationName))
        return jsonify(operations)
    
    except Exception as e:
        return handleExceptions(e)

@app.route('/apiv2/<operationName>', methods=['POST'])
@app.route('/apiv2/<subscriptionId>/<operationName>', methods=['POST'])
@app.route('/apim/<serviceName>/<planName>/<operationName>', methods=['POST'])
@app.route('/apiv2/operations/<predecessorOperationId>/<operationName>', methods=['POST'])
@app.route('/api/<subscriptionId>/operations/<predecessorOperationId>/<operationName>', methods=['POST'])
def executeOperation(operationName, predecessorOperationId = 'na', subscriptionId = 'default', serviceName = None, planName = None):
    
    try:
        subscription = validateAPIKeyAndGetSubscription(subscriptionId, serviceName, planName);
        apiVersion = getAPIVersion(subscription);
        if apiVersion.PlanType == 'pipeline':
            if apiVersion.LinkedServiceType != 'AML':
                raise LunaServerException("No AML workspace found for subscription {}".format(subscriptionId))
            pipeline = AMLPipelineEndpoint.Get(apiVersion.Id, operationName)
            if not pipeline:
                raise LunaUserException(HTTPStatus.NOT_FOUND, "Operation {} is not supported.".format(operationName))
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            if predecessorOperationId != 'na':
                result = amlUtil.getOperationStatus(predecessorOperationId, subscription.Owner, subscription.SubscriptionId)
                if result["status"] != 'Completed':
                    raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation {} is not in Completed status.".format(predecessorOperationId))

            opId = amlUtil.submitPipelineRun(subscription, apiVersion, pipeline, request.json, predecessorOperationId = predecessorOperationId)

        elif apiVersion.PlanType == 'mlproject':
            if apiVersion.LinkedServiceType == "ADB":
                adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
                adbUtil = AzureDatabricksUtils(adbWorkspace)
                if predecessorOperationId != 'na':
                    result = adbUtil.getOperationStatus(predecessorOperationId, subscription.Owner, subscription.SubscriptionId)
                    if result["status"] != 'FINISHED':
                        raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation {} is not in FINISHED status.".format(predecessorOperationId))
                opId = adbUtil.runProject(subscription, apiVersion, operationName, request.json, predecessorOperationId = predecessorOperationId)
            elif apiVersion.LinkedServiceType == "AML":
                amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
                amlUtil = AzureMLUtils(amlWorkspace)
                if predecessorOperationId != 'na':
                    result = amlUtil.getOperationStatus(predecessorOperationId, subscription.Owner, subscription.SubscriptionId)
                    if result["status"] != 'Completed':
                        raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation {} is not in Completed status.".format(predecessorOperationId))
                opId = amlUtil.runProject(subscription, apiVersion, operationName, request.json, predecessorOperationId = predecessorOperationId)
        else:
            raise LunaUserException(HTTPStatus.NOT_FOUND, "No operation named {} published in the current AI service plan.".format(operationName))
            
        return jsonify({'operationId': opId})
    
    except Exception as e:
        return handleExceptions(e)

@app.route('/apiv2/operations/<operationId>/status', methods=['GET'])
@app.route('/apiv2/<subscriptionId>/operations/<operationId>/status', methods=['GET'])
def getOperationStatus(operationId, subscriptionId = 'default'):

    try:
        subscription = validateAPIKeyAndGetSubscription(subscriptionId);
        apiVersion = getAPIVersion(subscription);
        
        if apiVersion.LinkedServiceType == 'AML':
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            if apiVersion.PlanType == 'pipeline':
                runType = "azureml.PipelineRun"
            elif apiVersion.PlanType == 'mlproject':
                runType = "azureml.scriptrun"
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation not supported.")
            result = amlUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId, runType)

        elif apiVersion.LinkedServiceType == 'ADB':
            adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
            adbUtil = AzureDatabricksUtils(adbWorkspace)
            result = adbUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId)
        else:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation not supported.")

        return jsonify(result)
    except Exception as e:
        return handleExceptions(e)

@app.route('/apiv2/operations/<operationName>', methods=['GET'])
@app.route('/apiv2/<subscriptionId>/operations/<operationName>', methods=['GET'])
def listOperations(operationName, subscriptionId='default'):
    
    try:
        subscription = validateAPIKeyAndGetSubscription(subscriptionId);
        apiVersion = getAPIVersion(subscription);
        
        if apiVersion.LinkedServiceType == 'AML':
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            if apiVersion.PlanType == 'pipeline':
                runType = "azureml.PipelineRun"
            elif apiVersion.PlanType == 'mlproject':
                runType = "azureml.scriptrun"
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation not supported.")
            result = amlUtil.listAllOperations(operationName, subscription.Owner, subscription.SubscriptionId, runType)

        elif apiVersion.LinkedServiceType == 'ADB':
            adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
            adbUtil = AzureDatabricksUtils(adbWorkspace)
            result = adbUtil.listAllOperations(operationName, subscription.Owner, subscription.SubscriptionId)
        else:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation not supported.")

        return jsonify(result)
    except Exception as e:
        return handleExceptions(e)

@app.route('/apiv2/operations/<operationId>/output', methods=['GET'])
@app.route('/apiv2/<subscriptionId>/operations/<operationId>/output', methods=['GET'])
def getOperationOutput(operationId, subscriptionId = 'default'):
    try:
        subscription = validateAPIKeyAndGetSubscription(subscriptionId);
        apiVersion = getAPIVersion(subscription);
        outputType = request.args.get('output-type')
        if not outputType:
            outputType = "json"
            
        if apiVersion.LinkedServiceType == 'AML':
            if apiVersion.PlanType == 'pipeline':
                runType = "azureml.PipelineRun"
            elif apiVersion.PlanType == 'mlproject':
                runType = "azureml.scriptrun"
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation not supported.")
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            operation = amlUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId, runType)
            if operation["status"] != 'Completed':
                raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation {} is not in Completed status.".format(operationId))

            result = amlUtil.getOperationOutput(operationId, subscription.Owner, subscription.SubscriptionId, runType, outputType)
        elif apiVersion.LinkedServiceType == "ADB":
            if apiVersion.PlanType == 'mlproject':
                adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
                adbUtil = AzureDatabricksUtils(adbWorkspace)
                operation = adbUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId)
                if operation["status"] != 'FINISHED':
                    raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation {} is not in FINISHED status.".format(operationId))
                result = adbUtil.getOperationOutput(operationId, subscription.Owner, subscription.SubscriptionId, outputType)
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation is not supported.")
        else:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, "Operation is not supported.")
        
        if outputType == "file":
            with open(result, 'rb') as bites:
                return send_file(
                     io.BytesIO(bites.read()),
                     attachment_filename='outputs_{}.zip'.format(operationId),
                     mimetype='application/zip'
                )
        return jsonify(result)
    except Exception as e:
        return handleExceptions(e)

@app.route('/')
@app.route('/home')
def home():
    """Renders the home page."""
    return jsonify("This is an API Service.")


@app.route('/')
@app.route('/portal')
def portal():
    return redirect("https://luna-sa.azurewebsites.net/", code=302)