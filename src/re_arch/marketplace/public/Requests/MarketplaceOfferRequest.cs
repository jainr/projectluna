using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceOfferRequest
    {
        [JsonProperty(PropertyName = "offerId", Required = Required.Always)]
        public string OfferId { get; set; }

        [JsonProperty(PropertyName = "displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "isManualActivation", Required = Required.Default)]
        public bool IsManualActivation { get; set; }

        [JsonProperty(PropertyName = "offerParameters", Required = Required.Default)]
        public List<MarketplaceParameterRequest> OfferParameters { get; set; }

        [JsonProperty(PropertyName = "plans", Required = Required.Default)]
        public List<MarketplacePlanRequest> Plans { get; set; }

        [JsonProperty(PropertyName = "provisioningSteps", Required = Required.Default)]
        public List<BaseProvisioningStepRequest> ProvisioningSteps { get; set; }
    }
}
