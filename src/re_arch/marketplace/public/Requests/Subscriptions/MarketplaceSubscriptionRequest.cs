using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceSubscriptionRequest
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "publisherId", Required = Required.Always)]
        public string PublisherId { get; set; }

        [JsonProperty(PropertyName = "offerId", Required = Required.Always)]
        public string OfferId { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "planId", Required = Required.Always)]
        public string PlanId { get; set; }

        [JsonProperty(PropertyName = "ownerId", Required = Required.Always)]
        public string OwnerId { get; set; }

        [JsonProperty(PropertyName = "token", Required = Required.Default)]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "inputParameters", Required = Required.Default)]
        public List<MarketplaceSubscriptionParameterRequest> InputParameters { get; set; }
    }
}
