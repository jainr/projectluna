using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client
{
    public class RBACQueryRequest
    {
        public static string example = JsonConvert.SerializeObject(new RBACQueryRequest()
        {
            Uid = "b46324b3-6a92-4e35-84be-fa1b2919af69",
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
