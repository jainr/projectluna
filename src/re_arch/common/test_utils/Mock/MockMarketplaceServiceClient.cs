using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Test
{
    public class MockMarketplaceServiceClient : IMarketplaceServiceClient
    {
        public async Task ActivateMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers)
        {
            return;
        }

        public Task<string> CreateMarketplaceOfferAsync(string offerId, string offer, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateMarketplacePlanAsync(string offerId, string planId, string plan, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateMarketplaceSubscriptionAsync(Guid subscriptionId, string subscription, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateOfferParameterAsync(string offerId, string paramName, string param, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<MarketplaceOffer> CreateOrUpdateMarketplaceOfferFromTemplateAsync(string name, string template, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateProvisioningStepAsync(string offerId, string stepName, string step, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMarketplaceOfferAsync(string offerId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMarketplacePlanAsync(string offerId, string planId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOfferParameterAsync(string offerId, string paramName, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProvisioningStepAsync(string offerId, string stepName, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetMarketplaceOfferAsync(string offerId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetMarketplaceParametersAsync(string offerId, string planId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetMarketplacePlanAsync(string offerId, string planId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetOfferParameterAsync(string offerId, string paramName, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetProvisioningStepAsync(string offerId, string stepName, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> ListMarketplaceOffersAsync(LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> ListMarketplacePlansAsync(string offerId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> ListMarketplaceSubscriptionsAsync(LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> ListOfferParametersAsync(string offerId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> ListProvisioningStepsAsync(string offerId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task PublishMarketplaceOfferAsync(string offerId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> ResolveMarketplaceTokenAsync(string token, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task UnsubscribeMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateMarketplaceOfferAsync(string offerId, string offer, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateMarketplacePlanAsync(string offerId, string planId, string plan, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateOfferParameterAsync(string offerId, string paramName, string param, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateProvisioningStepAsync(string offerId, string stepName, string step, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }
    }
}
