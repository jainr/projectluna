using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Luna.RBAC.Data.DataContracts;
using Luna.RBAC.Public.Client.DataContracts;
using Luna.RBAC.Clients;
using Luna.RBAC.Data.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Luna.RBAC.Public.Client.Enums;
using Luna.Common.Utils.HttpUtils;
using Luna.Common.Utils.LoggingUtils;
using System.Collections.Generic;

namespace Luna.RBAC
{
    /// <summary>
    /// The service maintains all RBAC rules
    /// </summary>
    public class RBACFunctions
    {
        private readonly IRBACCacheClient _cacheClient;
        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<RBACFunctions> _logger;

        public RBACFunctions(IRBACCacheClient cacheClient, ISqlDbContext dbContext, ILogger<RBACFunctions> logger)
        {
            this._cacheClient = cacheClient;
            this._dbContext = dbContext;
            this._logger = logger;
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
        ///     where T is <see cref="RoleAssignment"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignment.example"/>
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
                    var assignments = await _dbContext.RoleAssignments.ToListAsync();
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
        ///     <see cref="RoleAssignment"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignment.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of role assignment
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200"><see cref="RoleAssignment"/>Success</response>
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
                    var assignment = await DeserializeRequestBody<RoleAssignmentDb>(req);
                    assignment.CreatedTime = DateTime.UtcNow;
                    _dbContext.RoleAssignments.Add(assignment);
                    await _dbContext._SaveChangesAsync();
                    _cacheClient.AddRoleAssignment(assignment);
                    return new OkObjectResult(assignment);
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
        ///     <see cref="RoleAssignment"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignment.example"/>
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
                    var assignment = await DeserializeRequestBody<RoleAssignmentDb>(req);
                    var roleAssignments = await _dbContext.RoleAssignments.
                        Where(r => r.Uid == assignment.Uid && r.Role == assignment.Role).ToListAsync();

                    _dbContext.RoleAssignments.RemoveRange(roleAssignments);
                    await _dbContext._SaveChangesAsync();

                    _cacheClient.RemoveRoleAssignment(assignment);
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
        ///     <see cref="Ownership"/>
        ///     <example>
        ///         <value>
        ///             <see cref="Ownership.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of ownership
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200"><see cref="Ownership"/>Success</response>
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
                    var ownership = await DeserializeRequestBody<OwnershipDb>(req);
                    ownership.CreatedTime = DateTime.UtcNow;
                    _dbContext.Ownerships.Add(ownership);
                    await _dbContext._SaveChangesAsync();
                    _cacheClient.AssignOwnership(ownership);
                    return new OkObjectResult(ownership);
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
        ///     <see cref="Ownership"/>
        ///     <example>
        ///         <value>
        ///             <see cref="Ownership.example"/>
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
                    var ownership = await DeserializeRequestBody<OwnershipDb>(req);
                    var ownerships = await _dbContext.Ownerships.
                        Where(o => o.Uid == ownership.Uid && o.ResourceId == ownership.ResourceId).ToListAsync();
                    _dbContext.Ownerships.RemoveRange(ownerships);
                    await _dbContext._SaveChangesAsync();
                    _cacheClient.RemoveOwnership(ownership);
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
        ///     <see cref="RBACQuery"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RBACQuery.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of RBAC query
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="RBACQueryResult"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RBACQueryResult.example"/>
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
                    if (!_cacheClient.IsCacheInitialized())
                    {
                        var roleAssignments = await _dbContext.RoleAssignments.ToListAsync();
                        var ownerships = await _dbContext.Ownerships.ToListAsync();
                        _cacheClient.InitializeCache(roleAssignments, ownerships);
                    }

                    var query = await DeserializeRequestBody<RBACQuery>(req);
                    var result = new RBACQueryResult()
                    {
                        Query = query,
                        CanAccess = false,
                        Role = RBACRole.Unknown.ToString()
                    };

                    if (_cacheClient.IsSystemAdmin(query.Uid))
                    {
                        result.CanAccess = true;
                        result.Role = RBACRole.SystemAdmin.ToString();
                    }
                    else if (_cacheClient.IsPublisher(query.Uid))
                    {
                        if (_cacheClient.IsOwnedBy(query.Uid, query.ResourceId) ||
                            (!string.IsNullOrEmpty(query.Action) && RBACActions.PublisherAllowedActions.Contains(query.Action)))
                        {
                            result.CanAccess = true;
                            result.Role = RBACRole.Publisher.ToString();
                        }
                    }

                    _logger.LogInformation("User {0} with role {1} {2} access resource {3} with action {4}",
                        result.Query.Uid,
                        result.Role,
                        result.CanAccess ? "can" : "can not",
                        result.Query.ResourceId,
                        result.Query.Action ?? "All");

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

        private async Task<T> DeserializeRequestBody<T>(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            T obj = (T)JsonConvert.DeserializeObject(requestBody, typeof(T));
            return obj;
        }
    }
}
