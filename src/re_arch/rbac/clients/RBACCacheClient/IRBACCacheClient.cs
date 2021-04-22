using Luna.RBAC.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Clients
{
    /// <summary>
    /// The client interface to access RBAC cache
    /// </summary>
    public interface IRBACCacheClient
    {
        /// <summary>
        /// Add a role assignment to the RBAC cache
        /// </summary>
        /// <param name="assignment">The role assignment to be added</param>
        /// <returns>True if the role assignment is added, False if the role assignment already exists</returns>
        bool AddRoleAssignment(RoleAssignment assignment);

        /// <summary>
        /// Remove a role assignment from the RBAC cache
        /// </summary>
        /// <param name="assignment">The role assignment to be removed</param>
        /// <returns>True if the role assignment is removed, False if the role assignment doesn't exists</returns>
        bool RemoveRoleAssignment(RoleAssignment assignment);

        /// <summary>
        /// Check if the a user is a system admin
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <returns>True if the user is a system admin, False if the user is not a system admin or doesn't exist</returns>
        bool IsSystemAdmin(string uid);

        /// <summary>
        /// Check if the a user is a publisher
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <returns>True if the user is a publisher, False if the user is not a publisher or doesn't exist</returns>
        bool IsPublisher(string uid);

        /// <summary>
        /// Add a ownership assignment
        /// </summary>
        /// <param name="ownership">The ownership</param>
        /// <returns>True the ownership is assigned, False if the ownership is already assigned</returns>
        bool AssignOwnership(Ownership ownership);

        /// <summary>
        /// Remove a ownership assignment
        /// </summary>
        /// <param name="ownership">The ownership</param>
        /// <returns>True if the ownership assignment is remove, False if the ownership doesn't exist</returns>
        bool RemoveOwnership(Ownership ownership);

        /// <summary>
        /// Check if the a user owns a resource
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The resource id</param>
        /// <returns>True if the user owns the resource, False if the user doesn't own the resource</returns>
        bool IsOwnedBy(string uid, string resourceId);
    }
}
