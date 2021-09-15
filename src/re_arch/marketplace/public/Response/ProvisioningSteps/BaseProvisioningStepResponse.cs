using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class BaseProvisioningStepResponse
    {
        [JsonProperty(PropertyName = "offerId", Required = Required.Always)]
        public string OfferId { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "isSynchronized", Required = Required.Default)]
        public bool IsSynchronized { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "createdTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "lastUpdatedTime", Required = Required.Default)]
        public DateTime LastUpdatedTime { get; set; }
    }
}
