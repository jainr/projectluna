using Luna.Gallery.Public.Client.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Gallery.Data.Entities
{
    public class AzureMarketplaceSubscriptionDB
    {
        public AzureMarketplaceSubscriptionDB()
        {
            this.SaaSSubscriptionStatus = MarketplaceSubscriptionStatus.PENDING_FULFILLMENT_START;
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        public AzureMarketplaceSubscriptionDB(MarketplaceSubscription sub, string ownerId):
            this()
        {
            this.SubscriptionId = sub.Id;
            this.SubscriptionName = sub.Name;
            this.OfferId = sub.OfferId;
            this.PlanId = sub.PlanId;
            this.Publisher = sub.PublisherId;
            this.OwnerId = ownerId;
        }

        [Key]
        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public string OwnerId { get; set; }

        public string SaaSSubscriptionStatus { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public string Publisher { get; set; }

        public string ParameterSecretName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? ActivatedTime { get; set; }

        public DateTime? UnsubscribedTime { get; set; }
    }
}
