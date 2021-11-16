"""
Routes and views for the flask application.
"""

from datetime import datetime
from flask import render_template, send_file,redirect
from flask import jsonify, request
from Agent.Azure.AzureMLUtils import AzureMLUtils
from Agent.Azure.AzureDatabricksUtils import AzureDatabricksUtils
from datetime import datetime
from uuid import uuid4
import pathlib
from Agent.Data.AzureDatabricksWorkspace import AzureDatabricksWorkspace
from Agent.Data.PlanApplicationAPI import PlanApplicationAPI
from Agent.Data.AMLPipelineEndpoint import AMLPipelineEndpoint
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
import pandas as pd
from Agent.Constants.Enums import APIType, ComputeType, EndpointAuthType, AMLOperationStatus, ADBOperationStatus, OutputType
from Agent.Constants.Constants import Constants
from Agent.Constants.ErrorMessages import UserErrorMessage

def getToken():
    bearerToken = request.headers.get(Constants.AUTHORIZATION_HEADER)
    if not bearerToken or not bearerToken.startswith(Constants.BEARER_TOKEN_PREFIX):
        raise LunaUserException(HTTPStatus.FORBIDDEN, UserErrorMessage.AAD_TOKEN_REQUIRED)
    return bearerToken.replace(Constants.BEARER_TOKEN_PREFIX, "")

def handleExceptions(e):
    if isinstance(e, LunaUserException):
        return e.message, e.http_status_code.value
    else:
        app.logger.info(e)
        return UserErrorMessage.INTERNAL_SERVER_ERROR, 500

def getAPIVersion(subscription):
    version = request.args.get(Constants.API_VERSION_QUERY_PARAM_NAME)
    if not version:
        raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.API_VERSION_REQUIRED)

    apiVersion = APIVersion.Get(subscription.ApplicationName, subscription.APIName, version)
    if not apiVersion:
        raise LunaUserException(HTTPStatus.NOT_FOUND, UserErrorMessage.API_VERSION_NOT_EXIST)

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

def validateAPIKeyAndGetSubscription(applicationName, apiName, subscriptionId=Constants.DEFAULT_SUBSCRIPTION_ID):
    
    subscriptionKey = request.headers.get(Constants.API_KEY_HEADER)
    if subscriptionKey:
        sub = Subscription.GetByKey(subscriptionKey)
        if not sub:
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, UserErrorMessage.INVALID_API_KEY)
        if not PlanApplicationAPI.Exists(sub.PlanId, applicationName, apiName):
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, UserErrorMessage.API_NOT_EXIST.format(apiName, applicationName))
        if subscriptionId != Constants.DEFAULT_SUBSCRIPTION_ID and subscriptionId.lower() != sub.SubscriptionId.lower():
            raise LunaUserException(HTTPStatus.UNAUTHORIZED, UserErrorMessage.SUBSCRIPTION_NOT_EXIST.format(subscriptionId))
    else:
        pem = request.headers.get('X-ARR-ClientCert')
        if pem:
            cert = x509.load_pem_x509_certificate(convertOnelinePemtoPemData(pem), default_backend())

            #validate thumbprint
            if binascii.hexlify(cert.fingerprint(hashes.SHA1())).upper() != str.encode(os.environ['API_FRONTEND_CERT_THUMBPRINT']):
                raise LunaUserException(HTTPStatus.UNAUTHORIZED, UserErrorMessage.INVALID_CERT)
            if cert.issuer.rfc4514_string() != os.environ['API_FRONTEND_CERT_ISSUER']:
                raise LunaUserException(HTTPStatus.UNAUTHORIZED, UserErrorMessage.INVALID_CERT)
            if cert.subject.rfc4514_string() != os.environ['API_FRONTEND_CERT_SUBJECT']:
                raise LunaUserException(HTTPStatus.UNAUTHORIZED, UserErrorMessage.INVALID_CERT)
            if datetime.utcnow() > cert.not_valid_after or datetime.utcnow() < cert.not_valid_before:
                raise LunaUserException(HTTPStatus.UNAUTHORIZED, UserErrorMessage.INVALID_CERT)

            sub = Subscription()
            subscriptionId = request.headers.get('Luna-Subscription')
            if not subscriptionId:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, 'Subscription id id not available.')
            sub.SubscriptionId = subscriptionId
            user = request.headers.get('Luna-User')
            if not user:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, 'User id id not available.')
            sub.Owner = user
    #else:
    #    objectId = AuthenticationHelper.ValidateSignitureAndUser(getToken(), subscriptionId)
    #    sub = Subscription.Get(subscriptionId, objectId)
    #    if not sub:
    #        raise LunaUserException(HTTPStatus.NOT_FOUND, 'The subscription {} does not exist.'.format(subscriptionId))
    
    sub.ApplicationName = applicationName
    sub.APIName = apiName
    return sub

# This is just a demo function. DO NOT use in production
@app.route('/apiv2/<serviceName>/<apiName>/datasets/<datasetName>', methods=['GET'])
def getDateset(serviceName, apiName, datasetName, subscriptionId = Constants.DEFAULT_SUBSCRIPTION_ID):
    
    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
        if (apiVersion.APIType != APIType.dataset.name):
            raise LunaUserException(HTTPStatus.NOT_FOUND, "No dataset published in the current API.");

        df = pd.read_csv("https://xiwutestai.blob.core.windows.net/lunav2/BostonHousing/Boston_all_with_header.csv")
        result = df.to_json(orient="split")
        parsed = json.loads(result)
        return jsonify(parsed)
    except Exception as e:
        return handleExceptions(e)

@app.route('/apiv2/<serviceName>/<apiName>/models', methods=['GET'])
def listModels(serviceName, apiName, subscriptionId = Constants.DEFAULT_SUBSCRIPTION_ID):
    
    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
        if (apiVersion.APIType != APIType.model.name):
            raise LunaUserException(HTTPStatus.NOT_FOUND, UserErrorMessage.NO_MODEL_PUBLISHED);
        models = MLModel.ListAll(apiVersion.Id);

        return jsonify(models)
    except Exception as e:
        return handleExceptions(e)
    
@app.route('/apiv2/<serviceName>/<apiName>/models/<modelName>', methods=['GET'])
def getModel(serviceName, apiName, modelName, subscriptionId = Constants.DEFAULT_SUBSCRIPTION_ID):
    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
        if (apiVersion.APIType != APIType.model.name):
            raise LunaUserException(HTTPStatus.NOT_FOUND, UserErrorMessage.NO_MODEL_PUBLISHED);

        mlModel = MLModel.Get(apiVersion.Id, modelName)
        if apiVersion.LinkedServiceType == ComputeType.AML.name:
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            modelZipFilePath = amlUtil.downloadModel(mlModel)
        elif apiVersion.LinkedServiceType == ComputeType.ADB.name:
            adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
            adbUtil = AzureDatabricksUtils(adbWorkspace)
            modelZipFilePath = adbUtil.downloadModel(mlModel)
        else:
            raise LunaUserException(HTTPStatus.NOT_FOUND, UserErrorMessage.CAN_NOT_CONNECT_TO_MODEL_REPO);
        
        with open(modelZipFilePath, 'rb') as bites:
            return send_file(
                 io.BytesIO(bites.read()),
                 attachment_filename='model_{}.zip'.format(modelName),
                 mimetype=Constants.HTTP_CONTENT_TYPE_ZIP
            )
    except Exception as e:
        return handleExceptions(e)
    

@app.route('/apiv2/<serviceName>/<apiName>/predict', methods=['POST'])
def realtimePredict(serviceName, apiName, subscriptionId = Constants.DEFAULT_SUBSCRIPTION_ID):
    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
        if (apiVersion.APIType != APIType.endpoint.name):
            raise LunaUserException(HTTPStatus.NOT_FOUND, UserErrorMessage.NO_ENDPOINT_PUBLISHED)
    
        headers = {Constants.HTTP_CONTENT_TYPE_HEADER_NAME: Constants.HTTP_CONTENT_TYPE_JSON}

        if apiVersion.IsManualInputEndpoint:
            requestUrl = apiVersion.EndpointUrl
            if apiVersion.EndpointAuthType == EndpointAuthType.API_KEY.name:
                secret = key_vault_client.get_secret(apiVersion.EndpointAuthSecretName).value
                if apiVersion.EndpointAuthAddTo == 'HEADER':
                    headers[apiVersion.EndpointAuthKey] = secret
                elif apiVersion.EndpointAuthType == EndpointAuthType.QUERY_ARAMETER.name:
                    requestUrl = "{}?{}={}".format(requestUrl, apiVersion.EndpointAuthKey, secret)
                else:
                    raise LunaServerException("Unknow endpoint auth add-to target.")
            elif apiVersion.EndpointAuthType == EndpointAuthType.SERVICE_PRINCIPAL.name:
                raise LunaUserException(HTTPStatus.NOT_IMPLEMENTED, UUserErrorMessage.NOT_IMPLEMENTED.format("Service principal"))
        elif apiVersion.LinkedServiceType == ComputeType.AML.name:
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            endpoint = amlUtil.getEndpoint(apiVersion)
            requestUrl = endpoint.scoring_uri
            if endpoint.auth_enabled:
                if endpoint.token_auth_enabled:
                    key, refresh = endpoint.get_token()
                else:
                    key, secondaryKey = endpoint.get_keys()
                headers[Constants.AUTHORIZATION_HEADER] = "{}{}".format(Constants.BEARER_TOKEN_PREFIX, key)
        elif apiVersion.LinkedServiceType == ComputeType.ADB.name:
            adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
            adbUtil = AzureDatabricksUtils(adbWorkspace)
            requestUrl = "{}/model/{}/{}/invocations".format(adbWorkspace.WorkspaceUrl, apiVersion.EndpointName, apiVersion.EndpointVersion)
            headers[Constants.AUTHORIZATION_HEADER] = "{}{}".format(Constants.BEARER_TOKEN_PREFIX, adbUtil.getAccessToken())

        response = requests.post(requestUrl, json.dumps(request.json), headers=headers)
        if response.ok:
            return response.json(), response.status_code
        return response.text, response.status_code
    except Exception as e:
        return handleExceptions(e)
    
@app.route('/apiv2/<serviceName>/<apiName>/operations/metadata', methods=['GET'])
def listPublishedOperations(serviceName, apiName, subscriptionId = Constants.DEFAULT_SUBSCRIPTION_ID):
    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
        if apiVersion.APIType == APIType.pipeline.name:
            if apiVersion.LinkedServiceType != ComputeType.AML.name:
                raise LunaServerException("No AML workspace found for subscription {}".format(subscriptionId))
            operations = AMLPipelineEndpoint.ListAll(apiVersion.Id)
        elif apiVersion.APIType == APIType.mlproject.name:
            gitUtil = GitUtils(GitRepo.GetById(apiVersion.GitRepoId))
            operations = gitUtil.getEntryPoints(apiVersion.GitVersion)
        else:
            raise LunaUserException(HTTPStatus.NOT_FOUND, UserErrorMessage.NO_OPERATION_PUBLISHED)
        return jsonify(operations)
    
    except Exception as e:
        return handleExceptions(e)

@app.route('/apiv2/<serviceName>/<apiName>/<operationName>', methods=['POST'])
@app.route('/apiv2/<serviceName>/<apiName>/operations/<predecessorOperationId>/<operationName>', methods=['POST'])
def executeOperation(serviceName, apiName, operationName, predecessorOperationId = Constants.PREDECESSOR_OP_ID_NA, subscriptionId = Constants.DEFAULT_SUBSCRIPTION_ID):
    
    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
        if apiVersion.APIType == APIType.pipeline.name:
            if apiVersion.LinkedServiceType != ComputeType.AML.name:
                raise LunaServerException("No AML workspace found for subscription {}".format(subscriptionId))
            pipeline = AMLPipelineEndpoint.Get(apiVersion.Id, operationName)
            if not pipeline:
                raise LunaUserException(HTTPStatus.NOT_FOUND, UserErrorMessage.OPERATION_NOT_SUPPORTED)
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            if predecessorOperationId != Constants.PREDECESSOR_OP_ID_NA:
                result = amlUtil.getOperationStatus(predecessorOperationId, subscription.Owner, subscription.SubscriptionId)
                if result[Constants.OPERATION_STATUS_PARAMETER_NAME] != AMLOperationStatus.Completed.name:
                    raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_IN_STATUS.format(predecessorOperationId, AMLOperationStatus.Complete.name))

            opId = amlUtil.submitPipelineRun(subscription, apiVersion, pipeline, request.json, predecessorOperationId = predecessorOperationId)

        elif apiVersion.APIType == APIType.mlproject.name:
            if apiVersion.LinkedServiceType == ComputeType.ADB.name:
                adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
                adbUtil = AzureDatabricksUtils(adbWorkspace)
                if predecessorOperationId != Constants.PREDECESSOR_OP_ID_NA:
                    result = adbUtil.getOperationStatus(predecessorOperationId, subscription.Owner, subscription.SubscriptionId)
                    if result[Constants.OPERATION_STATUS_PARAMETER_NAME] != ADBOperationStatus.FINISHED.name:
                        raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_IN_STATUS.format(predecessorOperationId, ADBOperationStatus.FINISHED.name))
                opId = adbUtil.runProject(subscription, apiVersion, operationName, request.json, predecessorOperationId = predecessorOperationId)
            elif apiVersion.LinkedServiceType == ComputeType.AML.name:
                amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
                amlUtil = AzureMLUtils(amlWorkspace)
                if predecessorOperationId != Constants.PREDECESSOR_OP_ID_NA:
                    result = amlUtil.getOperationStatus(predecessorOperationId, subscription.Owner, subscription.SubscriptionId)
                    if result[Constants.OPERATION_STATUS_PARAMETER_NAME] != AMLOperationStatus.Complete.name:
                        raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_IN_STATUS.format(predecessorOperationId, AMLOperationStatus.Complete.name))
                opId = amlUtil.runProject(subscription, apiVersion, operationName, request.json, predecessorOperationId = predecessorOperationId)
        else:
            raise LunaUserException(HTTPStatus.NOT_FOUND, UserErrorMessage.NO_OPERATION_PUBLISHED)
            
        return jsonify({'operationId': opId})
    
    except Exception as e:
        return handleExceptions(e)

@app.route('/apiv2/<serviceName>/<apiName>/operations/<operationId>/status', methods=['GET'])
def getOperationStatus(serviceName, apiName, operationId, subscriptionId = Constants.DEFAULT_SUBSCRIPTION_ID):

    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
        
        if apiVersion.LinkedServiceType == ComputeType.AML.name:
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            if apiVersion.APIType == APIType.pipeline.name:
                runType = Constants.AML_PIPELINE_RUN_TYPE
            elif apiVersion.APIType == APIType.mlproject.name:
                runType = Constants.AML_SCRIPT_RUN_TYPE
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)
            result = amlUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId, runType)

        elif apiVersion.LinkedServiceType == ComputeType.ADB.name:
            adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
            adbUtil = AzureDatabricksUtils(adbWorkspace)
            result = adbUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId)
        else:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)

        return jsonify(result)
    except Exception as e:
        return handleExceptions(e)

@app.route('/apiv2/<serviceName>/<apiName>/operations/<operationName>', methods=['GET'])
def listOperations(serviceName, apiName, operationName, subscriptionId=Constants.DEFAULT_SUBSCRIPTION_ID):
    
    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
        
        if apiVersion.LinkedServiceType == ComputeType.AML.name:
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            if apiVersion.APIType == APIType.pipeline.name:
                runType = Constants.AML_PIPELINE_RUN_TYPE
            elif apiVersion.APIType == APIType.mlproject.name:
                runType = Constants.AML_SCRIPT_RUN_TYPE
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)
            result = amlUtil.listAllOperations(operationName, subscription.Owner, subscription.SubscriptionId, runType)

        elif apiVersion.LinkedServiceType == ComputeType.ADB.name:
            adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
            adbUtil = AzureDatabricksUtils(adbWorkspace)
            result = adbUtil.listAllOperations(operationName, subscription.Owner, subscription.SubscriptionId)
        else:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)

        return jsonify(result)
    except Exception as e:
        return handleExceptions(e)
    
@app.route('/apiv2/<serviceName>/<apiName>/operations/<operationId>/log', methods=['GET'])
def getOperationLog(serviceName, apiName, operationId, subscriptionId = Constants.DEFAULT_SUBSCRIPTION_ID):
    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
            
        if apiVersion.LinkedServiceType == ComputeType.AML.name:
            if apiVersion.APIType == APIType.pipeline.name:
                runType = Constants.AML_PIPELINE_RUN_TYPE
            elif apiVersion.APIType == APIType.mlproject.name:
                runType = Constants.AML_SCRIPT_RUN_TYPE
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            operation = amlUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId, runType)

            result = amlUtil.getOperationLog(operationId, subscription.Owner, subscription.SubscriptionId, runType)
        elif apiVersion.LinkedServiceType == ComputeType.ADB.name:
            if apiVersion.APIType == APIType.mlproject.name:
                adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
                adbUtil = AzureDatabricksUtils(adbWorkspace)
                operation = adbUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId)
                result = adbUtil.getOperationLog(operationId, subscription.Owner, subscription.SubscriptionId)
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)
        else:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)
        return {"log": result};
    except Exception as e:
        return handleExceptions(e)

@app.route('/apiv2/<serviceName>/<apiName>/operations/<operationId>/output', methods=['GET'])
def getOperationOutput(serviceName, apiName, operationId, subscriptionId = Constants.DEFAULT_SUBSCRIPTION_ID):
    try:
        subscription = validateAPIKeyAndGetSubscription(serviceName, apiName, subscriptionId);
        apiVersion = getAPIVersion(subscription);
        outputType = request.args.get(Constants.OUTPUT_TYPE_QUERY_PARAM_NAME)
        if not outputType:
            outputType = OutputType.json.name
            
        if apiVersion.LinkedServiceType == ComputeType.AML.name:
            if apiVersion.APIType == APIType.pipeline.name:
                runType = Constants.AML_PIPELINE_RUN_TYPE
            elif apiVersion.APIType == APIType.mlproject.name:
                runType = Constants.AML_SCRIPT_RUN_TYPE
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)
            amlWorkspace = AMLWorkspace.GetByIdWithSecrets(apiVersion.AMLWorkspaceId);
            amlUtil = AzureMLUtils(amlWorkspace)
            operation = amlUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId, runType)
            if operation[Constants.OPERATION_STATUS_PARAMETER_NAME] != AMLOperationStatus.Complete.name:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.NO_OPERATION_PUBLISHED.format(operationId, AMLOperationStatus.Complete.name))

            result = amlUtil.getOperationOutput(operationId, subscription.Owner, subscription.SubscriptionId, runType, outputType)
        elif apiVersion.LinkedServiceType == ComputeType.ADB.name:
            if apiVersion.APIType == APIType.mlproject.name:
                adbWorkspace = AzureDatabricksWorkspace.GetByIdWithSecrets(apiVersion.AzureDatabricksWorkspaceId)
                adbUtil = AzureDatabricksUtils(adbWorkspace)
                operation = adbUtil.getOperationStatus(operationId, subscription.Owner, subscription.SubscriptionId)
                if operation[Constants.OPERATION_STATUS_PARAMETER_NAME] != ADBOperationStatus.FINISHED.name:
                    raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.NO_OPERATION_PUBLISHED.format(operationId, ADBOperationStatus.FINISHED.name))
                result = adbUtil.getOperationOutput(operationId, subscription.Owner, subscription.SubscriptionId, outputType)
            else:
                raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)
        else:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, UserErrorMessage.OPERATION_NOT_SUPPORTED)
        
        if outputType == OutputType.file.name:
            with open(result, 'rb') as bites:
                return send_file(
                     io.BytesIO(bites.read()),
                     attachment_filename='outputs_{}.zip'.format(operationId),
                     mimetype=Constants.HTTP_CONTENT_TYPE_ZIP
                )
        elif outputType == OutputType.json.name:
            return jsonify(result)
        else:
            raise LunaUserException(HTTPStatus.BAD_REQUEST, "Output type {} is not supported.".format(outputType))
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