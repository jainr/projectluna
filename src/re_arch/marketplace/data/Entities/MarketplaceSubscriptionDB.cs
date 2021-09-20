using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class MarketplaceSubscriptionDB
    {
        public MarketplaceSubscriptionDB()
        {
            this.SaaSSubscriptionStatus = MarketplaceSubscriptionStatus.PENDING_FULFILLMENT_START;
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        [Key]
        public Guid SubscriptionId { get; set; }

        public string Name { get; set; }

        public string OwnerId { get; set; }

        public string SaaSSubscriptionStatus { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public long PlanPublishedByEventId { get; set; }

        public string PublisherId { get; set; }

        [NotMapped]
        public List<MarketplaceSubscriptionParameter> InputParameters { get; set; }

        public string ParameterSecretName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? ActivatedTime { get; set; }

        public DateTime? UnsubscribedTime { get; set; }
    }
}
