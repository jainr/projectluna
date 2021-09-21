using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Marketplace.Clients
{
    public interface IMarketplaceFunctionsImpl
    {
        Task<MarketplaceOfferResponse> CreateMarketplaceOfferAsync(string name, MarketplaceOfferRequest offer, LunaRequestHeaders headers);

        Task<MarketplaceOfferResponse> UpdateMarketplaceOfferAsync(string name, MarketplaceOfferRequest offer, LunaRequestHeaders headers);

        Task PublishMarketplaceOfferAsync(string name, LunaRequestHeaders headers);

        Task DeleteMarketplaceOfferAsync(string name, LunaRequestHeaders headers);

        Task<MarketplaceOfferResponse> GetMarketplaceOfferAsync(string name, LunaRequestHeaders headers);

        Task<List<MarketplaceOfferResponse>> ListMarketplaceOffersAsync(string userId, LunaRequestHeaders headers);

        Task<MarketplacePlanResponse> CreateMarketplacePlanAsync(string offerName, string planName, MarketplacePlanRequest plan, LunaRequestHeaders headers);

        Task<MarketplacePlanResponse> UpdateMarketplacePlanAsync(string offerName, string planName, MarketplacePlanRequest plan, LunaRequestHeaders headers);

        Task DeleteMarketplacePlanAsync(string offerName, string planName, LunaRequestHeaders headers);

        Task<MarketplacePlanResponse> GetMarketplacePlanAsync(string offerName, string planName, LunaRequestHeaders headers);

        Task<List<MarketplacePlanResponse>> ListMarketplacePlansAsync(string offerName, LunaRequestHeaders headers);

        Task<BaseProvisioningStepResponse> CreateProvisioningStepAsync(string offerName, string stepName, BaseProvisioningStepRequest step, LunaRequestHeaders headers);

        Task<BaseProvisioningStepResponse> UpdateProvisioningStepAsync(string offerName, string stepName, BaseProvisioningStepRequest step, LunaRequestHeaders headers);

        Task DeleteProvisioningStepAsync(string offerName, string stepName, LunaRequestHeaders headers);

        Task<BaseProvisioningStepResponse> GetProvisioningStepAsync(string offerName, string stepName, LunaRequestHeaders headers);

        Task<List<BaseProvisioningStepResponse>> ListProvisioningStepsAsync(string offerName, LunaRequestHeaders headers);

        Task<MarketplaceParameterResponse> CreateParameterAsync(string offerName, string parameterName, MarketplaceParameterRequest step, LunaRequestHeaders headers);

        Task<MarketplaceParameterResponse> UpdateParameterAsync(string offerName, string parameterName, MarketplaceParameterRequest step, LunaRequestHeaders headers);

        Task DeleteParameterAsync(string offerName, string parameterName, LunaRequestHeaders headers);

        Task<MarketplaceParameterResponse> GetParameterAsync(string offerName, string parameterName, LunaRequestHeaders headers);

        Task<List<MarketplaceParameterResponse>> ListParametersAsync(string offerName, LunaRequestHeaders headers);

        Task<MarketplaceSubscriptionResponse> ResolveMarketplaceSubscriptionAsync(string token, LunaRequestHeaders headers);

        Task<MarketplaceSubscriptionResponse> CreateMarketplaceSubscriptionAsync(Guid subscriptionId, MarketplaceSubscriptionRequest subRequest, LunaRequestHeaders headers);

        Task<MarketplaceSubscriptionResponse> UpdateMarketplaceSubscriptionAsync(Guid subscriptionId, MarketplaceSubscriptionRequest subRequest, LunaRequestHeaders headers);

        Task DeleteMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers);

        Task ActivateMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers);

        Task<MarketplaceSubscriptionResponse> GetMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers);

        Task<List<MarketplaceSubscriptionResponse>> ListMarketplaceSubscriptionsAsync(LunaRequestHeaders headers);

        Task<List<MarketplaceSubscriptionResponse>> ListMarketplaceSubscriptionDetailsAsync(LunaRequestHeaders headers);

        Task<List<MarketplaceParameterResponse>> ListInputParametersAsync(string offerId, LunaRequestHeaders headers);

    }
}
