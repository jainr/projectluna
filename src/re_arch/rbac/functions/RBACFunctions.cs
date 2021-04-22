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
using Luna.RBAC.Clients;
using Luna.RBAC.Data.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Luna.RBAC.Data.Enums;

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "roleassignment")] HttpRequest req)
        {
            var assignment = await DeserializeRequestBody<RoleAssignment>(req);
            assignment.CreatedTime = DateTime.UtcNow;
            _dbContext.RoleAssignments.Add(assignment);
            await _dbContext._SaveChangesAsync();
            _cacheClient.AddRoleAssignment(assignment);
            return new OkObjectResult(assignment);
        }

        [FunctionName("RemoveRoleAssignment")]
        public async Task<IActionResult> RemoveRoleAssignment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "roleassignment")] HttpRequest req)
        {
            var assignment = await DeserializeRequestBody<RoleAssignment>(req);
            var roleAssignments = await _dbContext.RoleAssignments.
                Where(r => r.Uid == assignment.Uid && r.Role == assignment.Role).ToListAsync();

            _dbContext.RoleAssignments.RemoveRange(roleAssignments);
            await _dbContext._SaveChangesAsync();

            _cacheClient.RemoveRoleAssignment(assignment);
            return new NoContentResult();
        }

        [FunctionName("AssignOwnership")]
        public async Task<IActionResult> AssignOwnership(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "ownership")] HttpRequest req)
        {
            var ownership = await DeserializeRequestBody<Ownership>(req);
            ownership.CreatedTime = DateTime.UtcNow;
            _dbContext.Ownerships.Add(ownership);
            await _dbContext._SaveChangesAsync();
            _cacheClient.AssignOwnership(ownership);
            return new OkObjectResult(ownership);
        }

        [FunctionName("RemoveOwnership")]
        public async Task<IActionResult> RemoveOwnership(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "ownership")] HttpRequest req)
        {
            var ownership = await DeserializeRequestBody<Ownership>(req);
            var ownerships = await _dbContext.Ownerships.
                Where(o => o.Uid == ownership.Uid && o.ResourceId == ownership.ResourceId).ToListAsync();
            _dbContext.Ownerships.RemoveRange(ownerships);
            await _dbContext._SaveChangesAsync();
            _cacheClient.RemoveOwnership(ownership);
            return new NoContentResult();
        }

        [FunctionName("CanAccess")]
        public async Task<IActionResult> CanAccess(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "canaccess")] HttpRequest req)
        {
            var query = await DeserializeRequestBody<RBACQuery>(req);
            var result = new RBACQueryResult()
            {
                Query = query,
                CanAccess = false,
                Role = RBACRoles.Unknown.ToString()
            };

            if (_cacheClient.IsSystemAdmin(query.Uid))
            {
                result.CanAccess = true;
                result.Role = RBACRoles.SystemAdmin.ToString();
            }
            else if (_cacheClient.IsPublisher(query.Uid))
            {
                if (_cacheClient.IsOwnedBy(query.Uid, query.ResourceId) ||
                    query.Action.Equals(RBACActions.CREATE_NEW_APPLICATION))
                {
                    result.CanAccess = true;
                    result.Role = RBACRoles.Publisher.ToString();
                }
            }

            return new OkObjectResult(result);
        }

        private async Task<T> DeserializeRequestBody<T>(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            T obj = (T)JsonConvert.DeserializeObject(requestBody, typeof(T));
            return obj;
        }
    }
}
