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
        /// Add an RBAC rule to the cache
        /// </summary>
        /// <param name="rule">The RBAC rule to be added</param>
        void AddRBACRuleToCache(RBACRule rule);
    }
}
