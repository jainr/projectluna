using Luna.Publish.Public.Client.DataContract;
using Luna.Publish.Public.Client.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.Entities
{
    public class AzureMarketplaceOfferDB
    {
        public AzureMarketplaceOfferDB()
        {

        }

        public AzureMarketplaceOfferDB(AzureMarketplaceOffer offer)
        {
            this.MarketplaceOfferId = offer.MarketplaceOfferId;
            this.DisplayName = offer.DisplayName;
            this.Description = offer.Description;
            this.IsManualActivation = offer.IsManualActivation;
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
            this.Status = MarketplaceOfferStatus.Draft.ToString();
        }

        public void Update(AzureMarketplaceOffer offer)
        {
            this.DisplayName = offer.DisplayName ?? this.DisplayName;
            this.Description = offer.Description ?? this.Description;
            this.IsManualActivation = offer.IsManualActivation;
            this.LastUpdatedTime = DateTime.UtcNow;
        }

        public void Publish()
        {
            this.LastUpdatedTime = DateTime.UtcNow;
            this.Status = MarketplaceOfferStatus.Published.ToString();
        }

        public void Delete()
        {
            this.DeletedTime = DateTime.UtcNow;
            this.Status = MarketplaceOfferStatus.Deleted.ToString();
            this.Status = MarketplaceOfferStatus.Deleted.ToString();
        }

        public AzureMarketplaceOffer ToAzureMarketplaceOffer()
        {
            return new AzureMarketplaceOffer()
            {
                MarketplaceOfferId = this.MarketplaceOfferId,
                DisplayName = this.DisplayName,
                Description = this.Description,
                Status = this.Status,
                IsManualActivation = this.IsManualActivation,
                CreatedTime = this.CreatedTime,
                LastUpdatedTime = this.LastUpdatedTime
            };
        }

        public AzureMarketplaceOffer ToAzureMarketplaceOfferEvent()
        {
            var offer = new AzureMarketplaceOffer()
            {
                MarketplaceOfferId = this.MarketplaceOfferId,
                DisplayName = this.DisplayName,
                Description = this.Description,
                Status = this.Status,
                IsManualActivation = this.IsManualActivation,
                CreatedTime = this.CreatedTime,
                LastUpdatedTime = this.LastUpdatedTime,
                Plans = new List<AzureMarketplacePlan>()
            };

            foreach(var plan in this.Plans)
            {
                offer.Plans.Add(plan.ToAzureMarketplacePlanEvent());
            }

            return offer;
        }

        [JsonIgnore]
        public long Id { get; set; }

        public string MarketplaceOfferId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? DeletedTime { get; set; }

        public bool IsManualActivation { get; set; }

        public List<AzureMarketplacePlanDB> Plans { get; set; }
    }
}
