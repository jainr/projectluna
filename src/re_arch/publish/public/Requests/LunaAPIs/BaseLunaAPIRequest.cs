using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client
{
    [JsonConverter(typeof(LunaAPIRequestJsonConverter))]
    public abstract class BaseLunaAPIRequest
    {

        [JsonProperty(PropertyName = "displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "advancedSettings", Required = Required.Default)]
        public string AdvancedSettings { get; set; }
    }
}
