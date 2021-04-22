using Luna.RBAC.Data.DataContracts;
using Luna.RBAC.Data.Entities;
using Luna.RBAC.Data.Enums;
using System;

namespace Luna.RBAC.Clients
{
    /// <summary>
    /// The client class to access RBAC cache
    /// </summary>
    public class RBACCacheClient : IRBACCacheClient
    {
        private RBACCache _cache;

        public RBACCacheClient()
        {
            _cache = new RBACCache();
        }

        /// <summary>
        /// Add a role assignment to the RBAC cache
        /// </summary>
        /// <param name="assignment">The role assignment to be added</param>
        /// <returns>True if the role assignment is added, False if the role assignment already exists</returns>
        public bool AddRoleAssignment(RoleAssignment assignment)
        {
            RBACRoles role;
            if (Enum.TryParse<RBACRoles>(assignment.Role, false, out role))
            {
                switch (role)
                {
                    case RBACRoles.SystemAdmin:
                        return AddSystemAdmin(assignment.Uid);
                    case RBACRoles.Publisher:
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
        public bool RemoveRoleAssignment(RoleAssignment assignment)
        {
            RBACRoles role;
            if (Enum.TryParse<RBACRoles>(assignment.Role, false, out role))
            {
                switch (role)
                {
                    case RBACRoles.SystemAdmin:
                        return RemoveSystemAdmin(assignment.Uid);
                    case RBACRoles.Publisher:
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
        public bool AssignOwnership(Ownership ownership)
        {
            return _cache.Ownership.Add(new RBACCachedOwnership(ownership.Uid, ownership.ResourceId));
        }

        /// <summary>
        /// Remove a ownership assignment
        /// </summary>
        /// <param name="ownership">The ownership</param>
        /// <returns>True if the ownership assignment is remove, False if the ownership doesn't exist</returns>
        public bool RemoveOwnership(Ownership ownership)
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
