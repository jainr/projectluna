using Luna.Common.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.RBAC.Public.Client
{
    public class RBACClient : RestClient, IRBACClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RBACClient> _logger;
        private readonly RBACClientConfiguration _config;

        [ActivatorUtilitiesConstructor]
        public RBACClient(IOptionsMonitor<RBACClientConfiguration> option,
            HttpClient httpClient,
            ILogger<RBACClient> logger) :
            base(option, httpClient, logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._config = option.CurrentValue ?? throw new ArgumentNullException(nameof(option.CurrentValue));
        }


        /// <summary>
        /// Add role assignment
        /// </summary>
        /// <param name="roleAssignment">The role assignment</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the role assignment is added, false otherwise</returns>
        public async Task<bool> AddRoleAssignment(RoleAssignmentRequest roleAssignment, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"roleassignments/add");

            var content = JsonConvert.SerializeObject(roleAssignment);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, content, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Remove a role assignment
        /// </summary>
        /// <param name="roleAssignment">The role assignment</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the role assignment is removed, false otherwise</returns>
        public async Task<bool> RemoveRoleAssignment(RoleAssignmentRequest roleAssignment, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"roleassignments/remove");

            var content = JsonConvert.SerializeObject(roleAssignment);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, content, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Add a user as admin
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="userName">The user name</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The workspace configuration</returns>
        public async Task<bool> AddAdmin(string uid, string userName, LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(uid, nameof(uid));
            ValidationUtils.ValidateStringValueLength(userName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(userName));

            var role = new RoleAssignmentRequest()
            {
                Uid = uid,
                UserName = userName,
                Role = RBACRole.SystemAdmin.ToString()
            };

            return await AddRoleAssignment(role, headers);
        }

        /// <summary>
        /// Remove an admin
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the admin is removed, false otherwise</returns>
        public async Task<bool> RemoveAdmin(string uid, LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(uid, nameof(uid));

            var role = new RoleAssignmentRequest()
            {
                Uid = uid,
                UserName = string.Empty,
                Role = RBACRole.SystemAdmin.ToString()
            };

            return await AddRoleAssignment(role, headers);
        }


        /// <summary>
        /// Add a user as publisher
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="userName">The user name</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the publisher is added, false otherwise</returns>
        public async Task<bool> AddPublisher(string uid, string userName, LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(uid, nameof(uid));
            ValidationUtils.ValidateStringValueLength(userName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(userName));

            var role = new RoleAssignmentRequest()
            {
                Uid = uid,
                UserName = userName,
                Role = RBACRole.Publisher.ToString()
            };

            return await AddRoleAssignment(role, headers);
        }

        /// <summary>
        /// Remove an publisher
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the publisher is removed, false otherwise</returns>
        public async Task<bool> RemovePublisher(string uid, LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(uid, nameof(uid));

            var role = new RoleAssignmentRequest()
            {
                Uid = uid,
                Role = RBACRole.Publisher.ToString()
            };

            return await RemoveRoleAssignment(role, headers);
        }

        /// <summary>
        /// Add a user as an application owner
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The application resource id</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the owner is added, false otherwise</returns>
        public async Task<bool> AddApplicationOwner(string uid, string resourceId, LunaRequestHeaders headers)
        {
            // TODO: validate resource id
            ValidationUtils.ValidateObjectId(uid, nameof(uid));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"ownerships/add");
            var ownership = new OwnershipRequest()
            {
                Uid = uid,
                ResourceId = resourceId
            };

            var content = JsonConvert.SerializeObject(ownership);

            var response = await SendRequest(HttpMethod.Post, uri, content, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Add a user as an application owner
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The application resource id</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the owner is added, false otherwise</returns>
        public async Task<bool> RemoveApplicationOwner(string uid, string resourceId, LunaRequestHeaders headers)
        {
            // TODO: validate resource id
            ValidationUtils.ValidateObjectId(uid, nameof(uid));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"ownerships/remove");
            var ownership = new OwnershipRequest()
            {
                Uid = uid,
                ResourceId = resourceId
            };

            var content = JsonConvert.SerializeObject(ownership);

            var response = await SendRequest(HttpMethod.Post, uri, content, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Check if a user can access specified resource
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The application resource id</param>
        /// <param name="action">The action</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>True if the user can access the resource, false otherwise</returns>
        public async Task<bool> CanAccess(string uid, string resourceId, string action, LunaRequestHeaders headers)
        {
            // TODO: validate resource id
            ValidationUtils.ValidateObjectId(uid, nameof(uid));
            if (action != null)
            {
                ValidationUtils.ValidateStringInList(action, RBACActions.ValidActions, nameof(action));
            }

            var result = await GetRBACQueryResult(uid, resourceId, action, headers);
            return result.CanAccess;
        }

        /// <summary>
        /// Check if a user can access specified resource
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The application resource id</param>
        /// <param name="action">The action</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The RBAC Query result</returns>
        public async Task<RBACQueryResultResponse> GetRBACQueryResult(string uid, string resourceId, string action, LunaRequestHeaders headers)
        {
            // TODO: validate resource id
            ValidationUtils.ValidateObjectId(uid, nameof(uid));
            if (action != null)
            {
                ValidationUtils.ValidateStringInList(action, RBACActions.ValidActions, nameof(action));
            }

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"canaccess");
            var query = new RBACQueryRequest()
            {
                Uid = uid,
                ResourceId = resourceId,
                Action = action
            };

            var content = JsonConvert.SerializeObject(query);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, content, headers);

            var responseContent = JsonConvert.DeserializeObject<RBACQueryResultResponse>(
                await response.Content.ReadAsStringAsync());

            return responseContent;
        }

        /// <summary>
        /// List all role assignments
        /// </summary>
        /// <param name="headers"></param>
        /// <returns>The role assignments</returns>
        public async Task<List<RoleAssignmentResponse>> ListRoleAssignments(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"roleassignments");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var responseContent = JsonConvert.DeserializeObject<List<RoleAssignmentResponse>>(
                await response.Content.ReadAsStringAsync());

            return responseContent;
        }
    }
}
