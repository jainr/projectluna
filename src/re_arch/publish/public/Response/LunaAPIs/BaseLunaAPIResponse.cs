using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public abstract class BaseLunaAPIResponse
    {
        public static string example = JsonConvert.SerializeObject(new RealtimeEndpointAPIResponse()
        {
            ApplicationName = "myapp",
            Name = "api",
            DisplayName = "My API",
            Type = "Realtime",
            Description = "This is my API"
        });

        [JsonProperty(PropertyName = "applicationName", Required = Required.Always)]
        public string ApplicationName { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

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
