using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceOffer
    {
        public static string example = JsonConvert.SerializeObject(new MarketplaceOffer
        {
            OfferId = "myoffer"
        });

        public MarketplaceOffer()
        {
            this.Plans = new List<MarketplacePlan>();
            this.Parameters = new List<MarketplaceParameter>();
            this.ProvisioningSteps = new List<MarketplaceProvisioningStep>();
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(OfferId,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(OfferId));

        }

        [JsonProperty(PropertyName = "OfferId", Required = Required.Always)]
        public string OfferId { get; set; }

        [JsonProperty(PropertyName = "Status", Required = Required.Default)]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "Properties", Required = Required.Always)]
        public MarketplaceOfferProp Properties { get; set; }

        [JsonProperty(PropertyName = "Plans", Required = Required.Always)]
        public List<MarketplacePlan> Plans { get; set; }

        [JsonProperty(PropertyName = "Parameters", Required = Required.Always)]
        public List<MarketplaceParameter> Parameters { get; set; }

        [JsonIgnore]
        public List<MarketplaceProvisioningStep> ProvisioningSteps { get; set; }

        [JsonProperty(PropertyName = "ProvisioningStepsSecretName", Required = Required.Default)]
        public string ProvisioningStepsSecretName { get; set; }
    }
}
