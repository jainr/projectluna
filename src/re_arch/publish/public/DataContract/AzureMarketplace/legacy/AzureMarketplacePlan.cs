using System;
using System.Runtime.Serialization;
using Luna.Common.Utils;
using Newtonsoft.Json;

namespace Luna.Publish.Public.Client
{
    public class AzureMarketplacePlan
    {
        public static string example = JsonConvert.SerializeObject(new AzureMarketplacePlan()
        {
            MarketplaceOfferId = "myoffer",
            MarketplacePlanId = "myplan",
            Description = "This is my plan",
            IsLocalDeployment = true,
            ManagementKitDownloadUrl = "https://aka.ms/lunamgmtkit"
        });

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(MarketplaceOfferId, 
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, 
                nameof(MarketplaceOfferId));

            ValidationUtils.ValidateStringValueLength(MarketplacePlanId,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(MarketplacePlanId));

            ValidationUtils.ValidateHttpsUrl(ManagementKitDownloadUrl, nameof(ManagementKitDownloadUrl));
            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));

        }

        [JsonProperty(PropertyName = "MarketplaceOfferId", Required = Required.Always)]
        public string MarketplaceOfferId { get; set; }

        [JsonProperty(PropertyName = "MarketplacePlanId", Required = Required.Always)]
        public string MarketplacePlanId { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "IsLocalDeployment", Required = Required.Default)]
        public bool IsLocalDeployment { get; set; }

        [JsonProperty(PropertyName = "ManagementKitDownloadUrl", Required = Required.Default)]
        public string ManagementKitDownloadUrl { get; set; }

        [JsonProperty(PropertyName = "ManagementKitDownloadUrlSecretName", Required = Required.Default)]
        public string ManagementKitDownloadUrlSecretName { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "LastUpdatedTime", Required = Required.Default)]
        public DateTime LastUpdatedTime { get; set; }
    }
}
