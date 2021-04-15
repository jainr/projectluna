using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Data.Entities
{
    /// <summary>
    /// The database entity for RBAC rules
    /// </summary>
    public class RBACRule
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string Uid { get; set; }

        public string Scope { get; set; }

        public string Action { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
