using Luna.Gallery.Public.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Gallery.Data
{
    public class AzureMarketplaceSubscriptionDB
    {
        public AzureMarketplaceSubscriptionDB()
        {
            this.SaaSSubscriptionStatus = MarketplaceSubscriptionStatus.PENDING_FULFILLMENT_START;
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        public AzureMarketplaceSubscriptionDB(MarketplaceSubscription sub, string ownerId, long planCreatedByEventId):
            this()
        {
            this.SubscriptionId = sub.Id;
            this.SubscriptionName = sub.Name;
            this.OfferId = sub.OfferId;
            this.PlanId = sub.PlanId;
            this.PlanCreatedByEventId = planCreatedByEventId;
            this.Publisher = sub.PublisherId;
            this.OwnerId = ownerId;
        }

        public MarketplaceSubscriptionInternal ToMarketplaceSubscriptionInternal()
        {
            return new MarketplaceSubscriptionInternal()
            { 
                PlanCreatedByEventId = this.PlanCreatedByEventId,
                Id = this.SubscriptionId,
                Name = this.SubscriptionName,
                OfferId = this.OfferId,
                PlanId = this.PlanId,
                SaaSSubscriptionStatus = this.SaaSSubscriptionStatus,
                PublisherId = this.Publisher,
                ParametersSecretName = this.ParameterSecretName
            };
        }

        [Key]
        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public string OwnerId { get; set; }

        public string SaaSSubscriptionStatus { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public long PlanCreatedByEventId { get; set; }

        public string Publisher { get; set; }

        public string ParameterSecretName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? ActivatedTime { get; set; }

        public DateTime? UnsubscribedTime { get; set; }
    }
}
