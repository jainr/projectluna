using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client
{
    public class RBACQueryResult
    {
        public static string example = JsonConvert.SerializeObject(new RBACQueryResult()
        {
            Query = new RBACQuery()
            {
                Uid = "b46324b3-6a92-4e35-84be-fa1b2919af69",
                ResourceId = "applications/myapp",
                Action = null
            },
            CanAccess = true,
            Role = "Admin"
        });

        [JsonProperty(PropertyName = "Query", Required = Required.Always)]
        public RBACQuery Query { get; set; }

        [JsonProperty(PropertyName = "CanAccess", Required = Required.Always)]
        public bool CanAccess { get; set; }

        [JsonProperty(PropertyName = "Role", Required = Required.Default)]
        public string Role { get; set; }
    }
}
