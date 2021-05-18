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
