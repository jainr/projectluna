using Luna.Publish.Public.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class AzureMarketplacePlanDB
    {
        public AzureMarketplacePlanDB()
        {

        }

        public AzureMarketplacePlanDB(long offerId, AzureMarketplacePlan plan)
        {
            this.OfferId = offerId;
            this.MarketplacePlanId = plan.MarketplacePlanId;
            this.Description = plan.Description;
            this.IsLocalDeployment = plan.IsLocalDeployment;
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        public void Update(AzureMarketplacePlan plan)
        {
            this.MarketplacePlanId = plan.MarketplacePlanId;
            this.Description = plan.Description;
            this.IsLocalDeployment = plan.IsLocalDeployment;
            this.LastUpdatedTime = DateTime.UtcNow;
        }

        public AzureMarketplacePlan ToAzureMarketplacePlan(string managementKitDownloadUrl = null)
        {
            return new AzureMarketplacePlan()
            {
                MarketplaceOfferId = this.Offer.MarketplaceOfferId,
                Description = this.Description,
                MarketplacePlanId = this.MarketplacePlanId,
                IsLocalDeployment = this.IsLocalDeployment,
                ManagementKitDownloadUrl = managementKitDownloadUrl,
                CreatedTime = this.CreatedTime,
                LastUpdatedTime = this.LastUpdatedTime
            };
        }

        public AzureMarketplacePlan ToAzureMarketplacePlanEvent()
        {
            return new AzureMarketplacePlan()
            {
                MarketplaceOfferId = this.Offer.MarketplaceOfferId,
                Description = this.Description,
                MarketplacePlanId = this.MarketplacePlanId,
                IsLocalDeployment = this.IsLocalDeployment,
                ManagementKitDownloadUrlSecretName = this.ManagementKitDownloadUrlSecretName,
                CreatedTime = this.CreatedTime,
                LastUpdatedTime = this.LastUpdatedTime
            };
        }

        [JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        public long OfferId { get; set; }

        public string MarketplacePlanId { get; set; }

        public string Description { get; set; }

        public bool IsLocalDeployment { get; set; }

        public string ManagementKitDownloadUrlSecretName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        [JsonIgnore]
        public AzureMarketplaceOfferDB Offer { get; set; }

    }
}
