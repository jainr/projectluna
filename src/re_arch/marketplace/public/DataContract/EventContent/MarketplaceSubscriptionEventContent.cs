using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceSubscriptionEventContent
    {

        public MarketplaceSubscriptionEventContent() : base()
        {
        }

        [JsonProperty(PropertyName = "PlanPublishedByEventId", Required = Required.Default)]
        public long PlanPublishedByEventId { get; set; }

        [JsonProperty(PropertyName = "ParametersSecretName", Required = Required.Default)]
        public string ParametersSecretName { get; set; }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "PublisherId", Required = Required.Always)]
        public string PublisherId { get; set; }

        [JsonProperty(PropertyName = "OwnerId", Required = Required.Always)]
        public string OwnerId { get; set; }

        [JsonProperty(PropertyName = "OfferId", Required = Required.Always)]
        public string OfferId { get; set; }

        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "SaaSSubscriptionStatus", Required = Required.Default)]
        public string SaaSSubscriptionStatus { get; set; }

        [JsonProperty(PropertyName = "PlanId", Required = Required.Always)]
        public string PlanId { get; set; }

        [JsonProperty(PropertyName = "AllowedCustomerOperations", Required = Required.Default)]
        public List<string> AllowedCustomerOperations { get; set; }

        [JsonProperty(PropertyName = "Token", Required = Required.Default)]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "InputParameters", Required = Required.Default)]
        public List<MarketplaceSubscriptionParameter> InputParameters { get; set; }
    }
}
