using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client.DataContracts
{
    public class RBACQueryResult
    {
        public static string example = JsonConvert.SerializeObject(new RBACQueryResult()
        {
            Query = new RBACQuery()
            {
                Uid = Guid.NewGuid().ToString(),
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
