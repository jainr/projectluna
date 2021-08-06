using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Luna.RBAC.Data;
using Luna.RBAC.Public.Client;
using Luna.RBAC.Clients;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Luna.Common.Utils;
using System.Collections.Generic;

namespace Luna.RBAC
{
    /// <summary>
    /// The service maintains all RBAC rules
    /// </summary>
    public class RBACFunctions
    {
        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<RBACFunctions> _logger;
        private readonly IRBACFunctionsImpl _functionImpl;

        public RBACFunctions(ISqlDbContext dbContext, ILogger<RBACFunctions> logger, IRBACFunctionsImpl functionImpl)
        {
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._functionImpl = functionImpl ?? throw new ArgumentNullException(nameof(functionImpl));
        }

        /// <summary>
        /// List role assignments
        /// </summary>
        /// <group>Role Assignment</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/roleassignments</url>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="RoleAssignmentRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignmentRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of role assignment
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListRoleAssignments")]
        public async Task<IActionResult> ListRoleAssignments(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "roleassignments")] HttpRequest req)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListRoleAssignments));

                try
                {
                    var assignments = await _functionImpl.ListRoleAssignmentsAsync(lunaHeaders);
                    return new OkObjectResult(assignments);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListRoleAssignments));
                }
            }
        }

        /// <summary>
        /// Add role assignment
        /// </summary>
        /// <group>Role Assignment</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/roleassignments/add</url>
        /// <param name="req" in="body">
        ///     <see cref="RoleAssignmentRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignmentRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of role assignment
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200"><see cref="RoleAssignmentResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignmentResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of role assignment
        ///         </summary>
        ///     </example>
        /// Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("AddRoleAssignment")]
        public async Task<IActionResult> AddRoleAssignment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "roleassignments/add")] HttpRequest req)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddRoleAssignment));

                try
                {
                    var assignment = await HttpUtils.DeserializeRequestBodyAsync<RoleAssignmentRequest>(req);

                    var response = await this._functionImpl.AddRoleAssignmentAsync(assignment, lunaHeaders);

                    return new OkObjectResult(response);
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

        /// <summary>
        /// Remove role assignment
        /// </summary>
        /// <group>Role Assignment</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/roleassignments/remove</url>
        /// <param name="req" in="body">
        ///     <see cref="RoleAssignmentRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignmentRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of role assignment
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("RemoveRoleAssignment")]
        public async Task<IActionResult> RemoveRoleAssignment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "roleassignments/remove")] HttpRequest req)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemoveRoleAssignment));

                try
                {
                    var assignment = await HttpUtils.DeserializeRequestBodyAsync<RoleAssignmentRequest>(req);
                    await this._functionImpl.RemoveRoleAssignmentAsync(assignment, lunaHeaders);
                    return new NoContentResult();
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

        /// <summary>
        /// Assign ownership
        /// </summary>
        /// <group>Ownership</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/ownership/add</url>
        /// <param name="req" in="body">
        ///     <see cref="OwnershipRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="OwnershipRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of ownership
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200"><see cref="OwnershipResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="OwnershipResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of ownership
        ///         </summary>
        ///     </example>
        ///     Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("AssignOwnership")]
        public async Task<IActionResult> AssignOwnership(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ownerships/add")] HttpRequest req)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AssignOwnership));

                try
                {
                    var ownership = await HttpUtils.DeserializeRequestBodyAsync<OwnershipRequest>(req);
                    var request = await this._functionImpl.AssignOwnershipAsync(ownership, lunaHeaders);

                    return new OkObjectResult(request);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AssignOwnership));
                }
            }
        }

        /// <summary>
        /// Remove ownership
        /// </summary>
        /// <group>Ownership</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/ownership/remove</url>
        /// <param name="req" in="body">
        ///     <see cref="OwnershipRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="OwnershipRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of ownership
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("RemoveOwnership")]
        public async Task<IActionResult> RemoveOwnership(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ownerships/remove")] HttpRequest req)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemoveOwnership));

                try
                {
                    var ownership = await HttpUtils.DeserializeRequestBodyAsync<OwnershipRequest>(req);

                    await this._functionImpl.RemoveOwnershipAsync(ownership, lunaHeaders);

                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RemoveOwnership));
                }
            }
        }

        /// <summary>
        /// Check if a user can access the specified resource and action
        /// </summary>
        /// <group>Access Control</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/canaccess</url>
        /// <param name="req" in="body">
        ///     <see cref="RBACQueryRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RBACQueryRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of RBAC query
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="RBACQueryResultResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RBACQueryResultResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of RBAC query result
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CanAccess")]
        public async Task<IActionResult> CanAccess(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "canaccess")] HttpRequest req)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CanAccess));

                try
                {
                    var query = await HttpUtils.DeserializeRequestBodyAsync<RBACQueryRequest>(req);
                    var result = await this._functionImpl.CanAccessAsync(query, lunaHeaders);
                    return new OkObjectResult(result);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CanAccess));
                }
            }
        }
    }
}
