using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class MarketplaceSubscriptionInternal : MarketplaceSubscription
    {

        public MarketplaceSubscriptionInternal() : base()
        {
        }

        [JsonProperty(PropertyName = "PlanCreatedByEventId", Required = Required.Default)]
        public long PlanCreatedByEventId { get; set; }

        [JsonProperty(PropertyName = "ParametersSecretName", Required = Required.Default)]
        public string ParametersSecretName { get; set; }

    }
}
