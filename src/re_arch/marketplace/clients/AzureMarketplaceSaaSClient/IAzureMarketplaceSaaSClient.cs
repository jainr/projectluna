using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Threading.Tasks;

namespace Luna.Marketplace.Clients
{
    public interface IAzureMarketplaceSaaSClient
    {
        /// <summary>
        /// Resolve a marketplace subscription from token
        /// </summary>
        /// <param name="token">The token</param>
        /// <param name="headers">The request headers</param>
        /// <returns>The marketplace subscription</returns>
        Task<MarketplaceSubscriptionResponse> ResolveMarketplaceSubscriptionAsync(string token, LunaRequestHeaders headers);

        /// <summary>
        /// Activate a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="planId">The plan id</param>
        /// <param name="headers">The request header</param>
        /// <returns></returns>
        Task ActivateMarketplaceSubscriptionAsync(
            Guid subscriptionId, 
            string planId,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The request header</param>
        /// <returns>The marketplace subscription</returns>
        Task<MarketplaceSubscriptionResponse> GetMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers);

        /// <summary>
        /// Unsubscribe a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The request header</param>
        /// <returns></returns>
        Task UnsubscribeMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers);
    }
}
