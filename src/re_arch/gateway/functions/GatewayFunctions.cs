using Luna.Common.LoggingUtils;
using Luna.Common.Utils.HttpUtils;
using Luna.Common.Utils.LoggingUtils;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Common.Utils.RestClients;
using Luna.Gallery.Public.Client.Clients;
using Luna.Gallery.Public.Client.DataContracts;
using Luna.Partner.PublicClient.Clients;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Luna.Publish.PublicClient.Clients;
using Luna.Publish.PublicClient.DataContract;
using Luna.PubSub.PublicClient.Clients;
using Luna.RBAC.Public.Client;
using Luna.RBAC.Public.Client.DataContracts;
using Luna.RBAC.Public.Client.Enums;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Luna.Gateway.Functions
{
    public class GatewayFunctions
    {
        private readonly ILogger<GatewayFunctions> _logger;
        private readonly IPartnerServiceClient _partnerServiceClient;
        private readonly IRBACClient _rbacClient;
        private readonly IPublishServiceClient _publishServiceClient;
        private readonly IPubSubServiceClient _pubSubServiceClient;
        private readonly IGalleryServiceClient _galleryServiceClient;

        public GatewayFunctions(IPartnerServiceClient partnerServiceClient,
            IRBACClient rbacClient,
            IPublishServiceClient publishServiceClient,
            IPubSubServiceClient pubSubServiceClient,
            IGalleryServiceClient galleryServiceClient,
            ILogger<GatewayFunctions> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._rbacClient = rbacClient ?? throw new ArgumentNullException(nameof(rbacClient));
            this._publishServiceClient = publishServiceClient ?? throw new ArgumentNullException(nameof(publishServiceClient));
            this._partnerServiceClient = partnerServiceClient ?? throw new ArgumentNullException(nameof(partnerServiceClient));
            this._pubSubServiceClient = pubSubServiceClient ?? throw new ArgumentNullException(nameof(pubSubServiceClient));
            this._galleryServiceClient = galleryServiceClient ?? throw new ArgumentNullException(nameof(galleryServiceClient));
        }

        [FunctionName("test")]
        public async Task<IActionResult> Test(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/test")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetEventStoreConnectionString));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPartnerService));
                }
            }
        }

        #region pubsub
        [FunctionName("GetEventStoreConnectionString")]
        public async Task<IActionResult> GetEventStoreConnectionString(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/eventstores/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetEventStoreConnectionString));

                try
                {

                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/eventstores", null, lunaHeaders))
                    {
                        var result = await _pubSubServiceClient.GetEventStoreConnectionStringAsync(name, lunaHeaders);
                        return new OkObjectResult(result);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetEventStoreConnectionString));
                }
            }
        }
        #endregion

        #region publish
        [FunctionName("RegenerateLunaApplicationMasterKeys")]
        public async Task<IActionResult> RegenerateLunaApplicationMasterKeys(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/applications/{name}/regeneratemasterkeys")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RegenerateLunaApplicationMasterKeys));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RegenerateLunaApplicationMasterKeys));
                }
            }

        }

        [FunctionName("ListLunaApplications")]
        public async Task<IActionResult> ListLunaApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/applications")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListLunaApplications));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListLunaApplications));
                }
            }
        }

        [FunctionName("GetLunaApplicationMasterKeys")]
        public async Task<IActionResult> GetLunaApplicationMasterKeys(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/applications/{name}/masterkeys")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetLunaApplicationMasterKeys));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetLunaApplicationMasterKeys));
                }
            }
        }


        [FunctionName("CreateLunaApplication")]
        public async Task<IActionResult> CreateLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateLunaApplication));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateLunaApplication));
                }
            }
        }

        [FunctionName("UpdateLunaApplication")]
        public async Task<IActionResult> UpdateLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateLunaApplication));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateLunaApplication));
                }
            }

        }


        [FunctionName("DeleteLunaApplication")]
        public async Task<IActionResult> DeleteLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteLunaApplication));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteLunaApplication));
                }
            }

        }

        [FunctionName("CreateLunaAPI")]
        public async Task<IActionResult> CreateLunaAPI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateLunaAPI));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateLunaAPI));
                }
            }

        }

        [FunctionName("UpdateLunaAPI")]
        public async Task<IActionResult> UpdateLunaAPI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateLunaAPI));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateLunaAPI));
                }
            }

        }


        [FunctionName("DeleteLunaAPI")]
        public async Task<IActionResult> DeleteLunaAPI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteLunaAPI));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteLunaAPI));
                }
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

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateLunaAPIVersion));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateLunaAPIVersion));
                }
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

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateLunaAPIVersion));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateLunaAPIVersion));
                }
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

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteLunaAPIVersion));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteLunaAPIVersion));
                }
            }

        }

        [FunctionName("PublishLunaApplication")]
        public async Task<IActionResult> PublishLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/applications/{name}/publish")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.PublishLunaApplication));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.PublishLunaApplication));
                }
            }

        }

        [FunctionName("GetLunaApplication")]
        public async Task<IActionResult> GetLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetLunaApplication));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetLunaApplication));
                }
            }

        }
        #endregion

        #region rbac
        [FunctionName("RemoveRoleAssignment")]
        public async Task<IActionResult> RemoveRoleAssignment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/rbac/roleassignments/remove")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemoveRoleAssignment));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RemoveRoleAssignment));
                }
            }

        }

        [FunctionName("AddRoleAssignment")]
        public async Task<IActionResult> AddRoleAssignment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/rbac/roleassignments/add")] HttpRequest req,
            ILogger log)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddRoleAssignment));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AddRoleAssignment));
                }
            }
        }
        #endregion

        #region partner
        [FunctionName("AddAzureMLService")]
        public async Task<IActionResult> AddAzureMLService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddAzureMLService));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AddAzureMLService));
                }
            }
        }

        [FunctionName("UpdateAzureMLService")]
        public async Task<IActionResult> UpdateAzureMLService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateAzureMLService));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateAzureMLService));
                }
            }

        }

        [FunctionName("RemoveAzureMLService")]
        public async Task<IActionResult> RemoveAzureMLService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemoveAzureMLService));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RemoveAzureMLService));
                }
            }

        }

        [FunctionName("ListAzureMLPartnerServices")]
        public async Task<IActionResult> ListAzureMLPartnerServices(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/azureml")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListAzureMLPartnerServices));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListAzureMLPartnerServices));
                }
            }

        }

        [FunctionName("GetPartnerService")]
        public async Task<IActionResult> GetPartnerService(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetPartnerService));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPartnerService));
                }
            }

        }
        #endregion

        #region gallery

        [FunctionName("GetPublishedApplication")]
        public async Task<IActionResult> GetPublishedApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}")] HttpRequest req,
            string appName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetPublishedApplication));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"publishedApplications/{appName}", null, lunaHeaders))
                    {
                        var app = await _galleryServiceClient.GetLunaApplication(appName, lunaHeaders);
                        return new OkObjectResult(app);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPublishedApplication));
                }
            }
        }

        [FunctionName("ListPublishedApplications")]
        public async Task<IActionResult> ListPublishedApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListPublishedApplications));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"publishedApplications", "list", lunaHeaders))
                    {
                        var appList = await _galleryServiceClient.ListLunaApplications(lunaHeaders);
                        return new OkObjectResult(appList);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListPublishedApplications));
                }
            }
        }

        [FunctionName("GetPublishedApplicationSwagger")]
        public async Task<IActionResult> GetPublishedApplicationSwagger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}/swagger")] HttpRequest req,
            string appName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetPublishedApplicationSwagger));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"publishedApplications/{appName}", null, lunaHeaders))
                    {
                        var swagger = await _galleryServiceClient.GetLunaApplicationSwagger(appName, lunaHeaders);
                        return new OkObjectResult(swagger);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPublishedApplicationSwagger));
                }
            }
        }

        [FunctionName("GetRecommendedPublishedApplications")]
        public async Task<IActionResult> GetRecommendedPublishedApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}/recommended")] HttpRequest req,
            string appName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetRecommendedPublishedApplications));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"publishedApplications/{appName}", null, lunaHeaders))
                    {
                        var appList = await _galleryServiceClient.GetRecommendedLunaApplications(appName, lunaHeaders);
                        return new OkObjectResult(appList);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetRecommendedPublishedApplications));
                }
            }
        }

        [FunctionName("CreateSubscription")]
        public async Task<IActionResult> CreateSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "gallery/applications/{appName}/subscriptions/{subscriptionName}")] HttpRequest req,
            string appName,
            string subscriptionName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateSubscription));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions", "create", lunaHeaders))
                    {
                        var sub = await _galleryServiceClient.CreateLunaApplicationSubscription(appName, subscriptionName, lunaHeaders);
                        return new OkObjectResult(sub);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateSubscription));
                }
            }
        }

        [FunctionName("DeleteSubscription")]
        public async Task<IActionResult> DeleteSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetSubscription));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        await _galleryServiceClient.DeleteLunaApplicationSubscription(appName, subscriptionNameOrId, lunaHeaders);
                        return new NoContentResult();
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetSubscription));
                }
            }
        }

        [FunctionName("GetSubscription")]
        public async Task<IActionResult> GetSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetSubscription));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        var sub = await _galleryServiceClient.GetLunaApplicationSubscription(appName, subscriptionNameOrId, lunaHeaders);
                        return new OkObjectResult(sub);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetSubscription));
                }
            }
        }

        [FunctionName("ListSubscriptions")]
        public async Task<IActionResult> ListSubscriptions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}/subscriptions")] HttpRequest req,
            string appName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListSubscriptions));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions", "List", lunaHeaders))
                    {
                        var subList = await _galleryServiceClient.ListLunaApplicationSubscription(appName, lunaHeaders);
                        return new OkObjectResult(subList);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListSubscriptions));
                }
            }
        }

        [FunctionName("AddSubscriptionOwner")]
        public async Task<IActionResult> AddSubscriptionOwner(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/addOwner")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddSubscriptionOwner));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        var owner = await HttpUtils.DeserializeRequestBodyAsync<LunaApplicationSubscriptionOwner>(req);
                        owner = await _galleryServiceClient.AddLunaApplicationSubscriptionOwner(appName, 
                            subscriptionNameOrId, 
                            owner.UserId, 
                            owner.UserName, 
                            lunaHeaders);
                        return new OkObjectResult(owner);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AddSubscriptionOwner));
                }
            }
        }

        [FunctionName("RemoveSubscriptionOwner")]
        public async Task<IActionResult> RemoveSubscriptionOwner(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/removeOwner")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemoveSubscriptionOwner));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        var owner = await HttpUtils.DeserializeRequestBodyAsync<LunaApplicationSubscriptionOwner>(req);
                        owner = await _galleryServiceClient.RemoveLunaApplicationSubscriptionOwner(appName,
                            subscriptionNameOrId,
                            owner.UserId,
                            lunaHeaders);
                        return new OkObjectResult(owner);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RemoveSubscriptionOwner));
                }
            }
        }

        [FunctionName("RegenerateSubscriptionKey")]
        public async Task<IActionResult> RegenerateSubscriptionKey(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/regenerateKey")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RegenerateSubscriptionKey));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        if (!req.Query.ContainsKey("key-name"))
                        {
                            throw new LunaBadRequestUserException(
                                string.Format(ErrorMessages.MISSING_QUERY_PARAMETER, "key-name"),
                                UserErrorCode.MissingQueryParameter);
                        }
                        else
                        {
                            var keys = await _galleryServiceClient.RegenerateLunaApplicationSubscriptionKey(appName,
                                subscriptionNameOrId,
                                req.Query["key-name"].ToString(),
                                lunaHeaders);
                            return new OkObjectResult(keys);
                        }
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RegenerateSubscriptionKey));
                }
            }
        }

        [FunctionName("UpdateSubscriptionNotes")]
        public async Task<IActionResult> UpdateSubscriptionNotes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/updateNotes")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateSubscriptionNotes));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        var notes = await HttpUtils.DeserializeRequestBodyAsync<LunaApplicationSubscriptionNotes>(req);
                        notes = await _galleryServiceClient.UpdateLunaApplicationSubscriptionNotes(appName,
                            subscriptionNameOrId,
                            notes.Notes,
                            lunaHeaders);
                        return new OkObjectResult(notes);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateSubscriptionNotes));
                }
            }
        }
        #endregion
    }
}
