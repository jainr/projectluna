using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class MarketplaceSubscriptionEventMapper : IDataMapper<MarketplaceSubscriptionDB, MarketplaceSubscriptionEventContent>
    {
        public MarketplaceSubscriptionEventContent Map(MarketplaceSubscriptionDB subscription)
        {
            return new MarketplaceSubscriptionEventContent()
            {
                PlanCreatedByEventId = subscription.PlanCreatedByEventId,
                Id = subscription.SubscriptionId,
                Name = subscription.Name,
                OfferId = subscription.OfferId,
                PlanId = subscription.PlanId,
                SaaSSubscriptionStatus = subscription.SaaSSubscriptionStatus,
                PublisherId = subscription.PublisherId,
                ParametersSecretName = subscription.ParameterSecretName
            };
        }
    }
}
