using Luna.RBAC.Data.DataContracts;
using Luna.RBAC.Data.Entities;
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
        /// Add an RBAC rule to the cache
        /// </summary>
        /// <param name="rule">The RBAC rule to be added</param>
        public void AddRBACRuleToCache(RBACRule rule)
        {
            _cache.AddRBACRule(rule.Uid, rule.Scope, rule.Action);
        }
    }
}
