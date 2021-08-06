using Luna.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.RBAC.Public.Client
{
    public interface IRBACClient
    {

        /// <summary>
        /// Add role assignment
        /// </summary>
        /// <param name="roleAssignment">The role assignment</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the role assignment is added, false otherwise</returns>
        Task<bool> AddRoleAssignment(RoleAssignmentRequest roleAssignment, LunaRequestHeaders headers);

        /// <summary>
        /// Remove a role assignment
        /// </summary>
        /// <param name="roleAssignment">The role assignment</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the role assignment is removed, false otherwise</returns>
        Task<bool> RemoveRoleAssignment(RoleAssignmentRequest roleAssignment, LunaRequestHeaders headers);

        /// <summary>
        /// Add a user as admin
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="userName">The user name</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the admin is added, false otherwise</returns>
        Task<bool> AddAdmin(string uid, string userName, LunaRequestHeaders headers);

        /// <summary>
        /// Remove an admin
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the admin is removed, false otherwise</returns>
        Task<bool> RemoveAdmin(string uid, LunaRequestHeaders headers);


        /// <summary>
        /// Add a user as publisher
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="userName">The user name</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the publisher is added, false otherwise</returns>
        Task<bool> AddPublisher(string uid, string userName, LunaRequestHeaders headers);

        /// <summary>
        /// Remove an publisher
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the publisher is removed, false otherwise</returns>
        Task<bool> RemovePublisher(string uid, LunaRequestHeaders headers);

        /// <summary>
        /// Add a user as an application owner
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The application resource id</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the owner is added, false otherwise</returns>
        Task<bool> AddApplicationOwner(string uid, string resourceId, LunaRequestHeaders headers);

        /// <summary>
        /// Add a user as an application owner
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The application resource id</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>True if the owner is added, false otherwise</returns>
        Task<bool> RemoveApplicationOwner(string uid, string resourceId, LunaRequestHeaders headers);

        /// <summary>
        /// Check if a user can access specified resource
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The application resource id</param>
        /// <param name="action">The action</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>True if the user can access the resource, false otherwise</returns>
        Task<bool> CanAccess(string uid, string resourceId, string action, LunaRequestHeaders headers);

        /// <summary>
        /// Check if a user can access specified resource
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The application resource id</param>
        /// <param name="action">The action</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The RBAC Query result</returns>
        Task<RBACQueryResultResponse> GetRBACQueryResult(string uid, string resourceId, string action, LunaRequestHeaders headers);

        /// <summary>
        /// List all role assignments
        /// </summary>
        /// <param name="headers"></param>
        /// <returns>The role assignments</returns>
        Task<List<RoleAssignmentResponse>> ListRoleAssignments(LunaRequestHeaders headers);
    }
}
