using Luna.Common.LoggingUtils;
using Luna.Common.Utils.HttpUtils;
using Luna.Common.Utils.LoggingUtils;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Common.Utils.RestClients;
using Luna.Partner.PublicClient.Clients;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Luna.Publish.PublicClient.Clients;
using Luna.Publish.PublicClient.DataContract;
using Luna.RBAC.Public.Client;
using Luna.RBAC.Public.Client.DataContracts;
using Luna.RBAC.Public.Client.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Luna.Gateway.Functions
{
    public class GatewayFunctions
    {
        private readonly ILogger<GatewayFunctions> _logger;
        private readonly IPartnerServiceClient _partnerServiceClient;
        private readonly IRBACClient _rbacClient;
        private readonly IPublishServiceClient _publishServiceClient;

        public GatewayFunctions(IPartnerServiceClient partnerServiceClient,
            IRBACClient rbacClient,
            IPublishServiceClient publishServiceClient,
            ILogger<GatewayFunctions> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._rbacClient = rbacClient ?? throw new ArgumentNullException(nameof(rbacClient));
            this._publishServiceClient = publishServiceClient ?? throw new ArgumentNullException(nameof(publishServiceClient));
            this._partnerServiceClient = partnerServiceClient ?? throw new ArgumentNullException(nameof(partnerServiceClient));
        }

        [FunctionName("test")]
        public async Task<IActionResult> Test(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/test")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"rbac", null, lunaHeaders))
                {
                    return new OkObjectResult(req.Headers["X-MS-CLIENT-PRINCIPAL-ID"].ToString());
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("GetLunaApplicationEventStoreConnectionString")]
        public async Task<IActionResult> GetLunaApplicationEventStoreConnectionString(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/eventstores/applicationevents")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/eventstores", null, lunaHeaders))
                {
                    var result = await _publishServiceClient.GetEventStoreConnectionString("ApplicationEvents", lunaHeaders);
                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("RegenerateLunaApplicationMasterKeys")]
        public async Task<IActionResult> RegenerateLunaApplicationMasterKeys(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/applications/{name}/regeneratemasterkeys")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                var keyName = "";
                if (req.Query.ContainsKey(PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME))
                {
                    keyName = req.Query[PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME].ToString();
                }
                else
                {
                    throw new LunaBadRequestUserException(
                        string.Format(ErrorMessages.MISSING_QUERY_PARAMETER, PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME),
                        UserErrorCode.MissingQueryParameter);
                }

                if (!keyName.Equals(PublishQueryParameterConstants.PRIMARY_KEY_NAME) && 
                    !keyName.Equals(PublishQueryParameterConstants.SECONDARY_KEY_NAME))
                {
                    throw new LunaBadRequestUserException(
                        string.Format(ErrorMessages.INVALID_QUERY_PARAMETER_VALUE, PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME),
                        UserErrorCode.MissingQueryParameter);
                }

                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                {
                    var result = await _publishServiceClient.RegenerateApplicationMasterKeys(name, keyName, lunaHeaders);
                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("ListLunaApplications")]
        public async Task<IActionResult> ListLunaApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/applications")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (string.IsNullOrEmpty(lunaHeaders.UserId))
                {
                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }

                var rbacResult = await _rbacClient.GetRBACQueryResult(lunaHeaders.UserId, RBACActions.LIST_APPLICATIONS, null, lunaHeaders);

                if (rbacResult.CanAccess)
                {
                    var result = await _publishServiceClient.ListLunaApplications(rbacResult.Role.Equals(RBACRole.SystemAdmin.ToString()), lunaHeaders);
                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("GetLunaApplicationMasterKeys")]
        public async Task<IActionResult> GetLunaApplicationMasterKeys(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/applications/{name}/masterkeys")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                {
                    var result = await _publishServiceClient.GetApplicationMasterKeys(name, lunaHeaders);
                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }


        [FunctionName("CreateLunaApplication")]
        public async Task<IActionResult> CreateLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", RBACActions.CREATE_NEW_APPLICATION, lunaHeaders))
                {
                    var content = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _publishServiceClient.CreateLunaApplication(name, content, lunaHeaders);
                    if (await _rbacClient.AddApplicationOwner(lunaHeaders.UserId, $"/applications/{name}", lunaHeaders))
                    {
                        return new OkObjectResult(result);
                    }
                    else
                    {
                        // If failed to add owner, delete the application and throw exception
                        await _publishServiceClient.DeleteLunaApplication(name, lunaHeaders);
                        throw new LunaServerException($"Failed to add ownership for application {name}. Owner user id is {lunaHeaders.UserId}.");
                    }
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("UpdateLunaApplication")]
        public async Task<IActionResult> UpdateLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                {
                    var content = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _publishServiceClient.UpdateLunaApplication(name, content, lunaHeaders);
                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }


        [FunctionName("DeleteLunaApplication")]
        public async Task<IActionResult> DeleteLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                {
                    await _publishServiceClient.DeleteLunaApplication(name, lunaHeaders);
                    await _rbacClient.RemoveApplicationOwner(lunaHeaders.UserId, $"/applications/{name}", lunaHeaders);
                    return new NoContentResult();
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("CreateLunaAPI")]
        public async Task<IActionResult> CreateLunaAPI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                {
                    var content = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _publishServiceClient.CreateLunaAPI(appName, apiName, content, lunaHeaders);
                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("UpdateLunaAPI")]
        public async Task<IActionResult> UpdateLunaAPI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                {
                    var content = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _publishServiceClient.UpdateLunaAPI(appName, apiName, content, lunaHeaders);
                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }


        [FunctionName("DeleteLunaAPI")]
        public async Task<IActionResult> DeleteLunaAPI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                {
                    await _publishServiceClient.DeleteLunaAPI(appName, apiName, lunaHeaders);
                    return new NoContentResult();
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("CreateLunaAPIVersion")]
        public async Task<IActionResult> CreateLunaAPIVersion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/applications/{appName}/apis/{apiName}/versions/{versionName}")] HttpRequest req,
            string appName,
            string apiName,
            string versionName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                {
                    var content = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _publishServiceClient.CreateLunaAPIVersion(appName, apiName, versionName, content, lunaHeaders);
                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("UpdateLunaAPIVersion")]
        public async Task<IActionResult> UpdateLunaAPIVersion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/applications/{appName}/apis/{apiName}/versions/{versionName}")] HttpRequest req,
            string appName,
            string apiName,
            string versionName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                {
                    var content = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _publishServiceClient.UpdateLunaAPIVersion(appName, apiName, versionName, content, lunaHeaders);
                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }


        [FunctionName("DeleteLunaAPIVersion")]
        public async Task<IActionResult> DeleteLunaAPIVersion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/applications/{appName}/apis/{apiName}/versions/{versionName}")] HttpRequest req,
            string appName,
            string apiName,
            string versionName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                {
                    await _publishServiceClient.DeleteLunaAPIVersion(appName, apiName, versionName, lunaHeaders);
                    return new NoContentResult();
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("PublishLunaApplication")]
        public async Task<IActionResult> PublishLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/applications/{name}/publish")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                {
                    var comments = "No comment.";
                    if (req.Query.ContainsKey("comments"))
                    {
                        comments = req.Query["comments"].ToString();
                    }
                    await _publishServiceClient.PublishLunaApplication(name, comments, lunaHeaders);
                    return new NoContentResult();
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("GetLunaApplication")]
        public async Task<IActionResult> GetLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) && 
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                {
                    var result = await _publishServiceClient.GetLunaApplication(name, lunaHeaders);

                    return new OkObjectResult(result);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("RemoveRoleAssignment")]
        public async Task<IActionResult> RemoveRoleAssignment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/rbac/roleassignments/remove")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"rbac", null, lunaHeaders))
                {
                    var roleAssignment = await HttpUtils.DeserializeRequestBodyAsync<RoleAssignment>(req);

                    if (roleAssignment.Uid.Equals(lunaHeaders.UserId) && 
                        roleAssignment.Role.Equals(RBACRole.SystemAdmin.ToString()))
                    {
                        throw new LunaConflictUserException(ErrorMessages.CAN_NOT_REMOVE_YOUR_OWN_ACCOUNT_FROM_ADMN);
                    }

                    var result = await _rbacClient.RemoveRoleAssignment(roleAssignment, lunaHeaders);

                    if (result)
                    {
                        return new NoContentResult();
                    }
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("AddRoleAssignment")]
        public async Task<IActionResult> AddRoleAssignment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/rbac/roleassignments/add")] HttpRequest req,
            ILogger log)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"rbac", null, lunaHeaders))
                {
                    var roleAssignment = await HttpUtils.DeserializeRequestBodyAsync<RoleAssignment>(req);
                    var result = await _rbacClient.AddRoleAssignment(roleAssignment, lunaHeaders);

                    if (result)
                    {
                        return new NoContentResult();
                    }
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);

            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("AddAzureMLService")]
        public async Task<IActionResult> AddAzureMLService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"partnerservices", null, lunaHeaders))
                {
                    var config = await HttpUtils.DeserializeRequestBodyAsync<AzureMLWorkspaceConfiguration>(req);
                    await _partnerServiceClient.RegisterAzureMLWorkspace(name, config, lunaHeaders);
                    return new OkObjectResult(config);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("UpdateAzureMLService")]
        public async Task<IActionResult> UpdateAzureMLService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"partnerservices", null, lunaHeaders))
                {
                    var config = await HttpUtils.DeserializeRequestBodyAsync<AzureMLWorkspaceConfiguration>(req);
                    await _partnerServiceClient.UpdateAzureMLWorkspace(name, config, lunaHeaders);
                    return new OkObjectResult(config);
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("RemoveAzureMLService")]
        public async Task<IActionResult> RemoveAzureMLService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, $"partnerservices", null, lunaHeaders))
                {
                    if (await _partnerServiceClient.DeleteAzureMLWorkspace(name, lunaHeaders))
                    {
                        return new NoContentResult();
                    }
                }

                throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("ListAzureMLPartnerServices")]
        public async Task<IActionResult> ListAzureMLPartnerServices(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/azureml")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", null, lunaHeaders))
                {
                    var config = await _partnerServiceClient.ListAzureMLWorkspaces(lunaHeaders);
                    return new OkObjectResult(config);
                }

                throw new LunaUnauthorizedUserException(
                    string.Format(ErrorMessages.CAN_NOT_PERFORM_OPERATION));
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        [FunctionName("GetPartnerService")]
        public async Task<IActionResult> GetPartnerService(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            try
            {
                if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                    await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", null, lunaHeaders))
                {
                    var config = await _partnerServiceClient.GetAzureMLWorkspaceConfiguration(name, lunaHeaders);
                    return new OkObjectResult(config);
                }

                throw new LunaUnauthorizedUserException(
                    string.Format(ErrorMessages.PARTNER_SERVICE_DOES_NOT_EXIST, name));
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }
    }
}
