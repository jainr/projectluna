using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client.DataContracts
{
    public class RBACQuery
    {
        public static string example = JsonConvert.SerializeObject(new RBACQuery()
        {
            Uid = Guid.NewGuid().ToString(),
            ResourceId = "applications/myapp",
            Action = null
        });

        [JsonProperty(PropertyName = "Uid", Required = Required.Always)]
        public string Uid { get; set; }

        [JsonProperty(PropertyName = "ResourceId", Required = Required.Always)]
        public string ResourceId { get; set; }

        [JsonProperty(PropertyName = "Action", Required = Required.Default)]
        public string Action { get; set; }
    }
}
