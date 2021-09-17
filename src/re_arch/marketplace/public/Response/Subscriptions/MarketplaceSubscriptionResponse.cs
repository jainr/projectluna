using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceSubscriptionResponse
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "publisherId", Required = Required.Always)]
        public string PublisherId { get; set; }

        [JsonProperty(PropertyName = "offerId", Required = Required.Always)]
        public string OfferId { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "saaSSubscriptionStatus", Required = Required.Default)]
        public string SaaSSubscriptionStatus { get; set; }

        [JsonProperty(PropertyName = "planId", Required = Required.Always)]
        public string PlanId { get; set; }

        [JsonProperty(PropertyName = "allowedCustomerOperations", Required = Required.Default)]
        public List<string> AllowedCustomerOperations { get; set; }

        [JsonProperty(PropertyName = "createdTime", Required = Required.Always)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "lastUpdatedTime", Required = Required.Always)]
        public DateTime LastUpdatedTime { get; set; }

        [JsonProperty(PropertyName = "unsubscribedTime", Required = Required.Default)]
        public DateTime? UnsubscribedTime { get; set; }

    }
}
