using Luna.Common.Utils;
using Luna.Marketplace.Clients;
using Luna.Marketplace.Data;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Test
{
    public class MockAzureMarketplaceSaaSClient : IAzureMarketplaceSaaSClient
    {
        private readonly List<MarketplaceSubscriptionRequest> _mockSubscriptions;
        private readonly IDataMapper<MarketplaceSubscriptionRequest, MarketplaceSubscriptionResponse, MarketplaceSubscriptionDB> _dataMapper;

        public MockAzureMarketplaceSaaSClient(List<MarketplaceSubscriptionRequest> mockSubscriptions)
        {
            this._mockSubscriptions = mockSubscriptions;
            this._dataMapper = new MarketplaceSubscriptionMapper();
        }

        public async Task ActivateMarketplaceSubscriptionAsync(Guid subscriptionId, string planId, LunaRequestHeaders headers)
        {
            return;
        }

        public async Task<MarketplaceSubscriptionResponse> GetMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers)
        {
            var sub = this._mockSubscriptions.SingleOrDefault(x => x.Id == subscriptionId);

            if (sub == null)
            {
                throw new LunaServerException($"Failed to get the subscription. StatusCode: 500");
            }

            return this._dataMapper.Map(this._dataMapper.Map(sub));

        }

        public async Task<MarketplaceSubscriptionResponse> ResolveMarketplaceSubscriptionAsync(string token, LunaRequestHeaders headers)
        {
            var sub = this._mockSubscriptions.SingleOrDefault(x => x.Token == token);

            if (sub == null)
            {
                throw new LunaServerException($"Failed to resolve the subscription. StatusCode: 500");
            }

            return this._dataMapper.Map(this._dataMapper.Map(sub));
        }

        public async Task UnsubscribeMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers)
        {
            return;
        }
    }
}
