using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class ResolvedMarketplaceSubscriptionMapper : IDataMapper<InternalMarketplaceSubscriptionResponse, MarketplaceSubscriptionResponse>
    {
        public MarketplaceSubscriptionResponse Map(InternalMarketplaceSubscriptionResponse resolvedSub)
        {
            var sub = new MarketplaceSubscriptionResponse()
            {
                Id = resolvedSub.Id,
                PublisherId = resolvedSub.PublisherId,
                OfferId = resolvedSub.OfferId,
                Name = resolvedSub.Name,
                SaaSSubscriptionStatus = resolvedSub.SaaSSubscriptionStatus,
                PlanId = resolvedSub.PlanId,
                AllowedCustomerOperations = resolvedSub.AllowedCustomerOperations
            };

            return sub;
        }
    }
}
