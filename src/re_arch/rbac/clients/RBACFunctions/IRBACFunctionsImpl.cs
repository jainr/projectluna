using Luna.Common.Utils;
using Luna.RBAC.Public.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.RBAC.Clients
{
    public interface IRBACFunctionsImpl
    {
        Task<List<RoleAssignmentResponse>> ListRoleAssignmentsAsync(LunaRequestHeaders lunaHeaders);

        Task<RoleAssignmentResponse> AddRoleAssignmentAsync(RoleAssignmentRequest roleAssignment, LunaRequestHeaders lunaHeaders);

        Task RemoveRoleAssignmentAsync(RoleAssignmentRequest roleAssignment, LunaRequestHeaders lunaHeaders);

        Task<OwnershipResponse> AssignOwnershipAsync(OwnershipRequest ownership, LunaRequestHeaders lunaHeaders);

        Task RemoveOwnershipAsync(OwnershipRequest ownership, LunaRequestHeaders lunaHeaders);

        Task<RBACQueryResultResponse> CanAccessAsync(RBACQueryRequest query, LunaRequestHeaders lunaHeaders);

    }
}
