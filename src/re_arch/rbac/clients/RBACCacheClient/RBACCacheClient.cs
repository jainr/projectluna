using Luna.RBAC.Data.DataContracts;
using Luna.RBAC.Data.Entities;
using Luna.RBAC.Public.Client.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.RBAC.Clients
{
    /// <summary>
    /// The client class to access RBAC cache
    /// </summary>
    public class RBACCacheClient : IRBACCacheClient
    {
        private static RBACCache _cache = new RBACCache();

        public RBACCacheClient()
        {
        }

        /// <summary>
        /// Check if the cache initialized
        /// </summary>
        /// <returns>True if the cache is already initialized, false otherwise</returns>
        public bool IsCacheInitialized()
        {
            return _cache.Initialized;
        }

        /// <summary>
        /// Initialize the RBAC cache
        /// </summary>
        /// <param name="roleAssignments">The role assignments</param>
        /// <param name="ownerships">The ownerships</param>
        public void InitializeCache(List<RoleAssignmentDb> roleAssignments, List<OwnershipDb> ownerships)
        {
            foreach (var roleAssignment in roleAssignments)
            {
                this.AddRoleAssignment(roleAssignment);
            }

            foreach (var ownership in ownerships)
            {
                this.AssignOwnership(ownership);
            }

            _cache.Initialized = true;
        }

        /// <summary>
        /// Add a role assignment to the RBAC cache
        /// </summary>
        /// <param name="assignment">The role assignment to be added</param>
        /// <returns>True if the role assignment is added, False if the role assignment already exists</returns>
        public bool AddRoleAssignment(RoleAssignmentDb assignment)
        {
            RBACRole role;
            if (Enum.TryParse<RBACRole>(assignment.Role, false, out role))
            {
                switch (role)
                {
                    case RBACRole.SystemAdmin:
                        return AddSystemAdmin(assignment.Uid);
                    case RBACRole.Publisher:
                        return AddPublisher(assignment.Uid);
                    default:
                        return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove a role assignment from the RBAC cache
        /// </summary>
        /// <param name="assignment">The role assignment to be removed</param>
        /// <returns>True if the role assignment is removed, False if the role assignment doesn't exists</returns>
        public bool RemoveRoleAssignment(RoleAssignmentDb assignment)
        {
            RBACRole role;
            if (Enum.TryParse<RBACRole>(assignment.Role, false, out role))
            {
                switch (role)
                {
                    case RBACRole.SystemAdmin:
                        return RemoveSystemAdmin(assignment.Uid);
                    case RBACRole.Publisher:
                        return RemovePublisher(assignment.Uid);
                    default:
                        return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the a user is a system admin
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <returns>True if the user is a system admin, False if the user is not a system admin or doesn't exist</returns>
        public bool IsSystemAdmin(string uid)
        {
            return _cache.SystemAdmins.Contains(uid);
        }

        /// <summary>
        /// Check if the a user is a publisher
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <returns>True if the user is a publisher, False if the user is not a publisher or doesn't exist</returns>
        public bool IsPublisher(string uid)
        {
            return _cache.Publishers.Contains(uid);
        }

        /// <summary>
        /// Add a ownership assignment
        /// </summary>
        /// <param name="ownership">The ownership</param>
        /// <returns>True the ownership is assigned, False if the ownership is already assigned</returns>
        public bool AssignOwnership(OwnershipDb ownership)
        {
            return _cache.Ownership.Add(new RBACCachedOwnership(ownership.Uid, ownership.ResourceId));
        }

        /// <summary>
        /// Remove a ownership assignment
        /// </summary>
        /// <param name="ownership">The ownership</param>
        /// <returns>True if the ownership assignment is remove, False if the ownership doesn't exist</returns>
        public bool RemoveOwnership(OwnershipDb ownership)
        {
            return _cache.Ownership.Remove(new RBACCachedOwnership(ownership.Uid, ownership.ResourceId));
        }

        /// <summary>
        /// Check if the a user owns a resource
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="resourceId">The resource id</param>
        /// <returns>True if the user owns the resource, False if the user doesn't own the resource</returns>
        public bool IsOwnedBy(string uid, string resourceId)
        {
            return _cache.Ownership.Contains(new RBACCachedOwnership(uid, resourceId));
        }

        /// <summary>
        /// Add a system admin
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <returns>True if the user is added as a system admin, False if the user is already a system admin</returns>
        private bool AddSystemAdmin(string uid)
        {
            return _cache.SystemAdmins.Add(uid);
        }

        /// <summary>
        /// Remove a system admin
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <returns>True if the user is remove from system admin, False if the user was not a system admin</returns>
        private bool RemoveSystemAdmin(string uid)
        {
            return _cache.SystemAdmins.Remove(uid);
        }

        /// <summary>
        /// Add a publisher
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <returns>True if the user is added as a publisher, False if the user is already a publisher</returns>
        private bool AddPublisher(string uid)
        {
            return _cache.Publishers.Add(uid);
        }

        /// <summary>
        /// Remove a publisher
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <returns>True if the user is remove from publisher, False if the user was not a publisher</returns>
        private bool RemovePublisher(string uid)
        {
            return _cache.Publishers.Remove(uid);
        }
    }
}
