using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Common.Utils;
using Newtonsoft.Json.Linq;

namespace Luna.Marketplace.Public.Client
{
    public interface IMarketplaceServiceClient
    {

        /// <summary>
        /// Create or update Azure marketplace offer from template
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="template">The offer template</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task<MarketplaceOffer> CreateOrUpdateMarketplaceOfferFromTemplateAsync(string name,
            string template,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="offer">The offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task<MarketplaceOffer> CreateMarketplaceOfferAsync(string name,
            MarketplaceOffer offer, 
            LunaRequestHeaders headers);

        /// <summary>
        /// Update an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="offer">The offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task<MarketplaceOffer> UpdateMarketplaceOfferAsync(string name,
            MarketplaceOffer offer,
            LunaRequestHeaders headers);

        /// <summary>
        /// Publish an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task PublishMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        Task<JObject> GetMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// List Azure marketplace offers
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        Task<List<MarketplaceOffer>> ListMarketplaceOffersAsync(LunaRequestHeaders headers);

        /// <summary>
        /// Delete an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns></returns>
        Task DeleteMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers);

        /// Create a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="plan">The plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        Task<MarketplacePlan> CreateMarketplacePlanAsync(string offerName,
            string planName,
            MarketplacePlan plan,
            LunaRequestHeaders headers);

        /// Update a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="plan">The plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        Task<MarketplacePlan> UpdateMarketplacePlanAsync(string offerName,
            string planName,
            MarketplacePlan plan,
            LunaRequestHeaders headers);

        /// Delete a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        Task DeleteMarketplacePlanAsync(string offerName,
            string planName,
            LunaRequestHeaders headers);

        /// Get a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan</returns>
        Task<MarketplacePlan> GetMarketplacePlanAsync(string offerName,
            string planName,
            LunaRequestHeaders headers);

        /// List plans in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plans</returns>
        Task<List<MarketplacePlan>> ListMarketplacePlansAsync(string offerName,
            LunaRequestHeaders headers);

        #region subscriptions

        /// <summary>
        /// Resolve a marketplace token
        /// </summary>
        /// <param name="token">The subscription token</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The resolved subscription</returns>
        Task<MarketplaceSubscription> ResolveMarketplaceTokenAsync(string token, LunaRequestHeaders headers);

        /// <summary>
        /// Create a markplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="subscription">The subscription</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The created subscription</returns>
        Task<MarketplaceSubscription> CreateMarketplaceSubscriptionAsync(
            Guid subscriptionId,
            MarketplaceSubscription subscription,
            LunaRequestHeaders headers);

        /// <summary>
        /// Activate a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        Task ActivateMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers);

        /// <summary>
        /// Unsubscribe a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        Task UnsubscribeMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers);

        /// <summary>
        /// Get a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        Task<MarketplaceSubscription> GetMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers);

        /// <summary>
        /// List marketplace subscriptions
        /// </summary>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        Task<List<MarketplaceSubscription>> ListMarketplaceSubscriptionsAsync(LunaRequestHeaders headers);

        /// <summary>
        /// Get parameters for the specified offer and plan
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan id</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The parameters</returns>
        Task<List<MarketplaceParameter>> GetMarketplaceParametersAsync(string offerId, string planId, LunaRequestHeaders headers);

        #endregion
    }
}
