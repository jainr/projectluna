using Luna.Gallery.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Clients
{
    public class MarketplaceSubscriptionResponse
    {
        public MarketplaceSubscriptionResponse()
        {
            AllowedCustomerOperations = new List<string>();
        }

        public MarketplaceSubscription ToMarketplaceSubscription()
        {
            var sub = new MarketplaceSubscription()
            {
                Id = this.Id,
                PublisherId = this.PublisherId,
                OfferId = this.OfferId,
                Name = this.Name, 
                SaaSSubscriptionStatus = this.SaaSSubscriptionStatus,
                PlanId = this.PlanId,
                AllowedCustomerOperations = this.AllowedCustomerOperations
            };

            return sub;
        }

        public Guid Id { get; set; }

        public string PublisherId { get; set; }

        public string OfferId { get; set; }

        public string Name { get; set; }

        public string SaaSSubscriptionStatus { get; set; }

        public string PlanId { get; set; }

        public List<string> AllowedCustomerOperations { get; set; }
    }
}
