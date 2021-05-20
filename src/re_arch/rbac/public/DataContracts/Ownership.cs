using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client.DataContracts
{
    public class Ownership
    {
        public static string example = JsonConvert.SerializeObject(new Ownership()
        {
            Uid = Guid.NewGuid().ToString(),
            ResourceId = "applications/myapp"
        });

        [JsonProperty(PropertyName = "Uid", Required = Required.Always)]
        public string Uid { get; set; }

        [JsonProperty(PropertyName = "ResourceId", Required = Required.Always)]
        public string ResourceId { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }
    }
}
