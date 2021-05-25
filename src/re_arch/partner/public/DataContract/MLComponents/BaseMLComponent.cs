using Luna.Publish.PublicClient.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.PublicClient.DataContract
{
    public class BaseMLComponent
    {
        public static string example = JsonConvert.SerializeObject(
            new BaseMLComponent("myendpoint", "My Endpoint", LunaAPIType.Realtime));

        public BaseMLComponent(LunaAPIType type)
        {
            this.Type = type;
        }
        public BaseMLComponent(string id, string displayName, LunaAPIType type)
        {
            this.Id = id;
            this.DisplayName = displayName;
            this.Type = type;
        }

        [JsonProperty(PropertyName = "Type", Required = Required.Always)]
        public LunaAPIType Type { get; set; }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }
    }
}
