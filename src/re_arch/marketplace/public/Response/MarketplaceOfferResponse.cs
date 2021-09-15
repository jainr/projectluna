using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceOfferResponse
    {
        [JsonProperty(PropertyName = "offerId", Required = Required.Always)]
        public string OfferId { get; set; }

        [JsonProperty(PropertyName = "displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "isManualActivation", Required = Required.Default)]
        public bool IsManualActivation { get; set; }

        [JsonProperty(PropertyName = "createdTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "lastUpdatedTime", Required = Required.Default)]
        public DateTime LastUpdatedTime { get; set; }

        [JsonProperty(PropertyName = "lastPublishedTime", Required = Required.Default)]
        public DateTime? LastPublishedTime { get; set; }

        [JsonProperty(PropertyName = "offerParameters", Required = Required.Default)]
        public List<MarketplaceParameterRequest> OfferParameters { get; set; }

        [JsonProperty(PropertyName = "plans", Required = Required.Default)]
        public List<MarketplacePlanRequest> Plans { get; set; }

        [JsonProperty(PropertyName = "provisioningSteps", Required = Required.Default)]
        public List<BaseProvisioningStepRequest> ProvisioningSteps { get; set; }
    }
}
