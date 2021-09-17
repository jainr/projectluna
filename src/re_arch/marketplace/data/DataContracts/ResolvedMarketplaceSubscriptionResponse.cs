using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class InternalMarketplaceSubscriptionResponse
    {
        public InternalMarketplaceSubscriptionResponse()
        {
            AllowedCustomerOperations = new List<string>();
        }

        public Guid Id { get; set; }

        public string PublisherId { get; set; }

        public string OfferId { get; set; }

        public string Name { get; set; }

        public string SaaSSubscriptionStatus { get; set; }

        public string PlanId { get; set; }

        public List<string> AllowedCustomerOperations { get; set; }
    }

    public class ResolvedMarketplaceSubscriptionResponse
    {
        public Guid Id { get; set; }

        public string SubscriptionName { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public int Quantity { get; set; }

        public InternalMarketplaceSubscriptionResponse Subscription { get; set; }
    }
}
