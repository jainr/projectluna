using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplacePlanRequest
    {
        [JsonProperty(PropertyName = "planId", Required = Required.Always)]
        public string PlanId { get; set; }

        [JsonProperty(PropertyName = "displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "mode", Required = Required.Always)]
        public string Mode { get; set; }

        [JsonProperty(PropertyName = "onSubscribe", Required = Required.Default)]
        public List<string> OnSubscribe { get; set; }

        [JsonProperty(PropertyName = "onUpdate", Required = Required.Default)]
        public List<string> OnUpdate { get; set; }

        [JsonProperty(PropertyName = "onSuspend", Required = Required.Default)]
        public List<string> OnSuspend { get; set; }

        [JsonProperty(PropertyName = "onDelete", Required = Required.Default)]
        public List<string> OnDelete { get; set; }

        [JsonProperty(PropertyName = "onPurge", Required = Required.Default)]
        public List<string> OnPurge { get; set; }

        [JsonProperty(PropertyName = "lunaApplicationName", Required = Required.Default)]
        public string LunaApplicationName { get; set; }
    }
}
