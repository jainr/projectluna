using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public class BaseAPIVersionRequest
    {
        [JsonProperty(PropertyName = "apiType", Required = Required.Always)]
        public string APIType { get; set; }

        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public string VersionType { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "advancedSettings", Required = Required.Default)]
        public string AdvancedSettings { get; set; }
    }
}
