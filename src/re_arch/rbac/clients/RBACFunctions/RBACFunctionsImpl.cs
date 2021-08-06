using Luna.Common.Utils;
using Luna.RBAC.Data;
using Luna.RBAC.Public.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.RBAC.Clients
{
    public class RBACFunctionsImpl : IRBACFunctionsImpl
    {

        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<RBACFunctionsImpl> _logger;
        private readonly IDataMapper<RoleAssignmentRequest, RoleAssignmentResponse, RoleAssignmentDb> _roleAssignmentMapper;
        private readonly IDataMapper<OwnershipRequest, OwnershipResponse, OwnershipDb> _ownershipMapper;

        public RBACFunctionsImpl(ISqlDbContext dbContext, 
            ILogger<RBACFunctionsImpl> logger,
            IDataMapper<RoleAssignmentRequest, RoleAssignmentResponse, RoleAssignmentDb> roleAssignmentMapper,
            IDataMapper<OwnershipRequest, OwnershipResponse, OwnershipDb> ownershipMapper)
        {
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._ownershipMapper = ownershipMapper ?? throw new ArgumentNullException(nameof(ownershipMapper));
            this._roleAssignmentMapper = roleAssignmentMapper ?? throw new ArgumentNullException(nameof(roleAssignmentMapper));
        }

        public async Task<List<RoleAssignmentResponse>> ListRoleAssignmentsAsync(LunaRequestHeaders lunaHeaders)
        {
            var assignments = await _dbContext.RoleAssignments.
                Select(x => _roleAssignmentMapper.Map(x)).
                ToListAsync();

            return assignments;
        }

        public async Task<RoleAssignmentResponse> AddRoleAssignmentAsync(RoleAssignmentRequest roleAssignment, LunaRequestHeaders lunaHeaders)
        {
            if (await _dbContext.RoleAssignments.AnyAsync(x => x.Uid == roleAssignment.Uid && x.Role == roleAssignment.Role))
            {
                throw new LunaConflictUserException(
                    string.Format(ErrorMessages.ROLE_ASSIGNMENT_ALREADY_EXIST, roleAssignment.Uid, roleAssignment.Role));
            }
            var roleAssignmentDb = this._roleAssignmentMapper.Map(roleAssignment);
            _dbContext.RoleAssignments.Add(roleAssignmentDb);
            await _dbContext._SaveChangesAsync();

            return this._roleAssignmentMapper.Map(roleAssignmentDb);
        }

        public async Task RemoveRoleAssignmentAsync(RoleAssignmentRequest roleAssignment, LunaRequestHeaders lunaHeaders)
        {
            var roleAssignments = await _dbContext.RoleAssignments.
                Where(r => r.Uid == roleAssignment.Uid && r.Role == roleAssignment.Role).ToListAsync();

            if (roleAssignments.Count > 0)
            {
                _dbContext.RoleAssignments.RemoveRange(roleAssignments);
            }
            else
            {
                throw new LunaNotFoundUserException(
                    string.Format(ErrorMessages.ROLE_ASSIGNMENT_DOES_NOT_EXIST, roleAssignment.Uid, roleAssignment.Role));
            }

            await _dbContext._SaveChangesAsync();
        }

        public async Task<OwnershipResponse> AssignOwnershipAsync(OwnershipRequest ownership, LunaRequestHeaders lunaHeaders)
        {
            if (await _dbContext.Ownerships.AnyAsync(x => x.Uid == ownership.Uid && x.ResourceId == ownership.ResourceId))
            {
                throw new LunaConflictUserException(
                    string.Format(ErrorMessages.OWNERSHIP_ALREADY_EXIST, ownership.Uid, ownership.ResourceId));
            }

            var ownershipDb = this._ownershipMapper.Map(ownership);
            _dbContext.Ownerships.Add(ownershipDb);
            await _dbContext._SaveChangesAsync();

            return this._ownershipMapper.Map(ownershipDb);

        }

        public async Task RemoveOwnershipAsync(OwnershipRequest ownership, LunaRequestHeaders lunaHeaders)
        {
            var ownerships = await _dbContext.Ownerships.
                Where(o => o.Uid == ownership.Uid && o.ResourceId == ownership.ResourceId).ToListAsync();

            if (ownerships.Count > 0)
            {
                _dbContext.Ownerships.RemoveRange(ownerships);
            }
            else
            {
                throw new LunaNotFoundUserException(
                    string.Format(ErrorMessages.OWNERSHIP_DOES_NOT_EXIST, ownership.Uid, ownership.ResourceId));
            }

            await _dbContext._SaveChangesAsync();
        }

        public async Task<RBACQueryResultResponse> CanAccessAsync(RBACQueryRequest query, LunaRequestHeaders lunaHeaders)
        {
            var result = new RBACQueryResultResponse()
            {
                Query = query,
                CanAccess = false,
                Role = RBACRole.Unknown.ToString()
            };

            if (await _dbContext.RoleAssignments.AnyAsync(x => x.Uid == query.Uid && x.Role == RBACRole.SystemAdmin.ToString()))
            {
                result.CanAccess = true;
                result.Role = RBACRole.SystemAdmin.ToString();
            }
            else if (await _dbContext.RoleAssignments.AnyAsync(x => x.Uid == query.Uid && x.Role == RBACRole.Publisher.ToString()))
            {
                if (await _dbContext.Ownerships.AnyAsync(x => x.Uid == query.Uid && x.ResourceId == query.ResourceId) ||
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

            return result;
        }
    }
}
