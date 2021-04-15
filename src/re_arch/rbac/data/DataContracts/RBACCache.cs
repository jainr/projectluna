using System;
using System.Collections;
using System.Collections.Generic;

namespace Luna.RBAC.Data.DataContracts
{
    /// <summary>
    /// The cached scopes. 
    /// Provides functions to add, remove and query scopes for a certain RBAC rule
    /// </summary>
    public class RBACCachedScopes
    {
        public const string WILDCARD_CHAR = "*";
        public RBACCachedScopes()
        {
            _exactScopes = new HashSet<string>();
            _wildcardScopes = new HashSet<string>();
        }

        private HashSet<string> _exactScopes;

        private HashSet<string> _wildcardScopes;

        /// <summary>
        /// Add a new scope
        /// </summary>
        /// <param name="scope">The scope to add</param>
        /// <returns>True if the scope is added. False if the scope already exists.</returns>
        public bool Add(string scope)
        {
            scope = scope.ToLower();
            if (scope.EndsWith(WILDCARD_CHAR))
            {
                // Remove the trailing wildcard character
                return _wildcardScopes.Add(scope.Substring(0, scope.Length - 1));
            }
            else
            {
                return _exactScopes.Add(scope);
            }
        }

        /// <summary>
        /// Remove a scope
        /// </summary>
        /// <param name="scope">The scope to remove</param>
        /// <returns>True if the scope is removed. False if the scope doesn't exist</returns>
        public bool Remove(string scope)
        {
            scope = scope.ToLower();
            if (scope.EndsWith(WILDCARD_CHAR))
            {
                return _wildcardScopes.Remove(scope.Substring(0, scope.Length - 1));
            }
            else
            {
                return _exactScopes.Remove(scope);
            }
        }

        /// <summary>
        /// Check if the input scope is covered by defined scopes
        /// </summary>
        /// <param name="scope">The input scope</param>
        /// <returns>True if it is covered. False otherwise.</returns>
        public bool Contains(string scope)
        {
            scope = scope.ToLower();
            if (_exactScopes.Contains(scope))
            {
                return true;
            }

            foreach (var wildcardScope in _wildcardScopes)
            {
                if (scope.StartsWith(wildcardScope))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// The cached RBAC actions
    /// </summary>
    public class RBACCachedActions
    {
        public RBACCachedActions()
        {
            this.CachedActions = new Dictionary<string, RBACCachedScopes>();
        }

        public Dictionary<string, RBACCachedScopes> CachedActions { get; set; }
    }

    /// <summary>
    /// The RBAC cache
    /// </summary>
    public class RBACCache
    {

        public RBACCache()
        {
            CachedUsers = new Dictionary<string, RBACCachedActions>();
        }

        // A dictionary with uid as keys
        public Dictionary<string, RBACCachedActions> CachedUsers { get; set; }

        /// <summary>
        /// Check if certain RBAC rule exist
        /// It could be covered by exact scope or wildcard scope
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="scope">The scope</param>
        /// <param name="action">The action</param>
        /// <returns>True if the rule exists, False otherwise.</returns>
        public bool HasRBACRule(string uid, string scope, string action)
        {
            if (CachedUsers.ContainsKey(uid))
            {
                var actions = CachedUsers[uid].CachedActions;
                if (actions.ContainsKey(action))
                {
                    return actions[action].Contains(scope);
                }
            }

            // Find no match
            return false;
        }

        /// <summary>
        /// Add a new RBAC rule
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="scope">The scope</param>
        /// <param name="action">The action</param>
        /// <returns>True if a new rule is added, false if the rule already exist</returns>
        public bool AddRBACRule(string uid, string scope, string action)
        {
            if (CachedUsers.ContainsKey(uid))
            {
                var actions = CachedUsers[uid].CachedActions;
                if (actions.ContainsKey(action))
                {
                    var scopes = actions[action];
                    if (scopes.Contains(scope))
                    {
                        // The RBAC rule already exist, return false
                        return false;
                    }
                    else
                    {
                        return scopes.Add(scope);
                    }
                }
                else
                {
                    var cachedScopes = new RBACCachedScopes();
                    cachedScopes.Add(scope);
                    actions.Add(action, cachedScopes);
                    return true;
                }
            }
            else
            {
                var cachedScopes = new RBACCachedScopes();
                cachedScopes.Add(scope);

                var cachedAction = new RBACCachedActions();
                cachedAction.CachedActions.Add(action, cachedScopes);
                CachedUsers.Add(uid, cachedAction);
                return true;
            }
        }

        /// <summary>
        /// Remove an RBAC rule
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="scope">The scope</param>
        /// <param name="action">The action</param>
        /// <returns>True if the rule is removed, False if the rule doesn't exist</returns>
        public bool RemoveRBACRule(string uid, string scope, string action)
        {
            if (CachedUsers.ContainsKey(uid))
            {
                var actions = CachedUsers[uid].CachedActions;
                if (actions.ContainsKey(action))
                {
                    var scopes = actions[action];
                    if (scopes.Contains(scope))
                    {
                        return scopes.Remove(scope);
                    }
                }
            }

            return false;
        }
    }
}
