using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Luna.Common.Utils;
using Newtonsoft.Json;

namespace Luna.Publish.Public.Client
{
    public class MarketplacePlan
    {
        public MarketplacePlan()
        {
            this.Parameters = new List<MarketplaceParameter>();
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(PlanId,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(PlanId));
        }

        [JsonProperty(PropertyName = "PlanId", Required = Required.Always)]
        public string PlanId { get; set; }

        [JsonProperty(PropertyName = "Properties", Required = Required.Default)]
        public MarketplacePlanProp Properties { get; set; }

        [JsonProperty(PropertyName = "Parameters", Required = Required.Always)]
        public List<MarketplaceParameter> Parameters { get; set; }
    }
}
