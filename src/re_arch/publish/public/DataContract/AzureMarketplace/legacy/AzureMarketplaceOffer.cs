using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Luna.Common.Utils;
using Newtonsoft.Json;

namespace Luna.Publish.Public.Client
{
    public class AzureMarketplaceOffer
    {
        public static string example = JsonConvert.SerializeObject(new AzureMarketplaceOffer()
        {
            MarketplaceOfferId = "myoffer",
            DisplayName = "My Offer",
            Description = "This is my offer",
            Status = MarketplaceOfferStatus.Draft.ToString(),
            CreatedTime = new DateTime(637588561931352800),
            LastUpdatedTime = new DateTime(637588561931352800),
            IsManualActivation = true
        });

        public AzureMarketplaceOffer()
        {
            this.Plans = new List<AzureMarketplacePlan>();
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(MarketplaceOfferId,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(MarketplaceOfferId));

            ValidationUtils.ValidateStringValueLength(DisplayName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(DisplayName));

            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));

        }

        [JsonProperty(PropertyName = "MarketplaceOfferId", Required = Required.Always)]
        public string MarketplaceOfferId { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Status", Required = Required.Default)]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "LastUpdatedTime", Required = Required.Default)]
        public DateTime LastUpdatedTime { get; set; }

        [JsonProperty(PropertyName = "DeletedTime", Required = Required.Default)]
        public DateTime? DeletedTime { get; set; }

        [JsonProperty(PropertyName = "IsManualActivation", Required = Required.Default)]
        public bool IsManualActivation { get; set; }

        [JsonProperty(PropertyName = "Plans", Required = Required.Default)]
        public List<AzureMarketplacePlan> Plans { get; set; }
    }
}
