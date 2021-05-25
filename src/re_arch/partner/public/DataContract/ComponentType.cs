using Luna.Publish.PublicClient.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.PublicClient.DataContract
{
    public class ComponentType
    {
        public static string example = JsonConvert.SerializeObject(
            new ComponentType(LunaAPIType.Realtime.ToString(), "Realtime endpoints"));

        public ComponentType(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }
    }
}
