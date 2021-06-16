using System;
using System.Collections;
using System.Collections.Generic;

namespace Luna.RBAC.Data
{
    /// <summary>
    /// The class defines RBAC ownership
    /// </summary>
    public class RBACCachedOwnership
    {
        public RBACCachedOwnership(string uid, string resourceId)
        {
            this.Uid = uid;
            this.ResourceId = resourceId;
        }

        public string Uid { get; set; }
        public string ResourceId { get; set; }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                RBACCachedOwnership ownership = (RBACCachedOwnership)obj;
                return ownership.Uid.Equals(this.Uid) && ownership.ResourceId.Equals(this.ResourceId);
            }
        }
        public override int GetHashCode()
        {
            return (this.Uid.GetHashCode() + this.ResourceId.GetHashCode()) / 2;
        }
    }

    /// <summary>
    /// The RBAC cache
    /// </summary>
    public class RBACCache
    {
        public RBACCache()
        {
            Initialized = false;
            SystemAdmins = new HashSet<string>();
            Publishers = new HashSet<string>();
            Ownership = new HashSet<RBACCachedOwnership>();
        }

        public bool Initialized { get; set; }

        public HashSet<string> SystemAdmins { get; set; }

        public HashSet<string> Publishers { get; set; }

        public HashSet<RBACCachedOwnership> Ownership { get; set; }

    }
}
