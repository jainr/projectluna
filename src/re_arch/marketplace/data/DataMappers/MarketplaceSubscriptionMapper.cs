using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Marketplace.Data
{

    public class MarketplaceSubscriptionMapper :
        IDataMapper<MarketplaceSubscriptionRequest, MarketplaceSubscriptionResponse, MarketplaceSubscriptionDB>
    {

        public MarketplaceSubscriptionDB Map(MarketplaceSubscriptionRequest request)
        {
            MarketplaceSubscriptionDB sub = new MarketplaceSubscriptionDB
            {
                SubscriptionId = request.Id,
                PublisherId = request.PublisherId,
                OfferId = request.OfferId,
                Name = request.Name,
                PlanId = request.PlanId,
                InputParameters = new List<MarketplaceSubscriptionParameter>(),
            };

            foreach (var param in request.InputParameters)
            {
                sub.InputParameters.Add(new MarketplaceSubscriptionParameter
                {
                    Name = param.Name,
                    Type = param.Type,
                    Value = param.Value,
                    IsSystemParameter = param.IsSystemParameter
                });
            }

            return sub;
        }

        public MarketplaceSubscriptionResponse Map(MarketplaceSubscriptionDB sub)
        {
            MarketplaceSubscriptionResponse response = new MarketplaceSubscriptionResponse
            {
                Id = sub.SubscriptionId,
                PublisherId = sub.PublisherId,
                OfferId = sub.OfferId,
                Name = sub.Name,
                PlanId = sub.PlanId,
                SaaSSubscriptionStatus = sub.SaaSSubscriptionStatus,
                CreatedTime = sub.CreatedTime,
                LastUpdatedTime = sub.LastUpdatedTime,
                UnsubscribedTime = sub.UnsubscribedTime,
            };

            return response;
        }
    }
}
