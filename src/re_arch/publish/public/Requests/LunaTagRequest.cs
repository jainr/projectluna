using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public class LunaTagRequest
    {
        [JsonProperty(PropertyName = "key", Required = Required.Default)]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "value", Required = Required.Default)]
        public string Value { get; set; }
    }
}
