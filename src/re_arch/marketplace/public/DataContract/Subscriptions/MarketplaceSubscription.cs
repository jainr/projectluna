using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceSubscription
    {
        public static string example = JsonConvert.SerializeObject(new MarketplaceSubscription()
        {
            Id = new Guid("7de85fed-9f81-44c0-ab36-d4540064200e"),
            PublisherId = "ms-ace",
            OfferId = "textanalytics",
            PlanId = "default",
            Name = "My sub",
            SaaSSubscriptionStatus = "Subscribed",
            AllowedCustomerOperations = new List<string>(new string[] { }),
            InputParameters = new List<MarketplaceSubscriptionParameter>()
        });

        public MarketplaceSubscription()
        {
            InputParameters = new List<MarketplaceSubscriptionParameter>();
        }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "PublisherId", Required = Required.Always)]
        public string PublisherId { get; set; }

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
