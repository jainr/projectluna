using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceSubscriptionEventContent : MarketplaceSubscription
    {

        public MarketplaceSubscriptionEventContent() : base()
        {
        }

        [JsonProperty(PropertyName = "PlanCreatedByEventId", Required = Required.Default)]
        public long PlanCreatedByEventId { get; set; }

        [JsonProperty(PropertyName = "ParametersSecretName", Required = Required.Default)]
        public string ParametersSecretName { get; set; }

    }
}
