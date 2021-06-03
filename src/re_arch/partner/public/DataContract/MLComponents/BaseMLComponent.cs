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
            this.Type = type.ToString();
        }

        public BaseMLComponent(string id, string name, LunaAPIType type)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type.ToString();
        }

        [JsonProperty(PropertyName = "Type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; }
    }
}
