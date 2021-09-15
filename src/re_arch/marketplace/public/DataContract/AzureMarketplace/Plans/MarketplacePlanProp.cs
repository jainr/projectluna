using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Luna.Common.Utils;
using Newtonsoft.Json;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplacePlanProp
    {
        public MarketplacePlanProp()
        {
            OnSubscribe = new List<string>();
            OnUpdate = new List<string>();
            OnSuspend = new List<string>();
            OnDelete = new List<string>();
            OnPurge = new List<string>();
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));
            ValidationUtils.ValidateEnum(Mode, typeof(MarketplacePlanMode), nameof(Mode));
        }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Mode", Required = Required.Always)]
        public string Mode { get; set; }

        [JsonProperty(PropertyName = "OnSubscribe", Required = Required.Default)]
        public List<string> OnSubscribe { get; set; }

        [JsonProperty(PropertyName = "OnUpdate", Required = Required.Default)]
        public List<string> OnUpdate { get; set; }

        [JsonProperty(PropertyName = "OnSuspend", Required = Required.Default)]
        public List<string> OnSuspend { get; set; }

        [JsonProperty(PropertyName = "OnDelete", Required = Required.Default)]
        public List<string> OnDelete { get; set; }

        [JsonProperty(PropertyName = "OnPurge", Required = Required.Default)]
        public List<string> OnPurge { get; set; }

        [JsonProperty(PropertyName = "lunaApplicationName", Required = Required.Default)]
        public string LunaApplicationName { get; set; }
    }
}
