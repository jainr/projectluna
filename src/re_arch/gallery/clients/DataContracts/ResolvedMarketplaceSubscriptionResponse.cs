using Luna.Gallery.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Clients
{
    public class ResolvedMarketplaceSubscriptionResponse
    {
        public MarketplaceSubscription ToMarketplaceSubscription()
        {
            if (this.Subscription !=null)
            {
                return this.Subscription.ToMarketplaceSubscription();
            }

            return null;
        }

        public Guid Id { get; set; }

        public string SubscriptionName { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public int Quantity { get; set; }

        public MarketplaceSubscriptionResponse Subscription { get; set; }
    }
}
