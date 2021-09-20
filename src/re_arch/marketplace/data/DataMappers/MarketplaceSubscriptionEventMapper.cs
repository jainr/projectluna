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
                PlanPublishedByEventId = subscription.PlanPublishedByEventId,
                Id = subscription.SubscriptionId,
                Name = subscription.Name,
                OfferId = subscription.OfferId,
                PlanId = subscription.PlanId,
                OwnerId = subscription.OwnerId,
                SaaSSubscriptionStatus = subscription.SaaSSubscriptionStatus,
                PublisherId = subscription.PublisherId,
                ParametersSecretName = subscription.ParameterSecretName
            };
        }
    }
}
