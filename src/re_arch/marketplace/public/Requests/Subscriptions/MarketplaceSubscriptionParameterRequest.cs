using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceSubscriptionParameterRequest
    {
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "value", Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "isSystemParameter", Required = Required.Always)]
        public bool IsSystemParameter { get; set; }
    }
}
