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
        /// <param name="offerId">Id of the offer</param>
        /// <param name="offer">The offer request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task<string> CreateMarketplaceOfferAsync(string offerId,
            string offer, 
            LunaRequestHeaders headers);

        /// <summary>
        /// Update an Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="offer">The offer request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task<string> UpdateMarketplaceOfferAsync(string offerId,
            string offer,
            LunaRequestHeaders headers);

        /// <summary>
        /// Publish an Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task PublishMarketplaceOfferAsync(string offerId,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get an Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        Task<string> GetMarketplaceOfferAsync(string offerId,
            LunaRequestHeaders headers);

        /// <summary>
        /// List Azure marketplace offers
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        Task<string> ListMarketplaceOffersAsync(LunaRequestHeaders headers);

        /// <summary>
        /// Delete an Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns></returns>
        Task DeleteMarketplaceOfferAsync(string offerId,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="planId">Id of the plan</param>
        /// <param name="plan">The plan request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        Task<string> CreateMarketplacePlanAsync(string offerId,
            string planId,
            string plan,
            LunaRequestHeaders headers);

        /// <summary>
        /// Update a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="planId">Id of the plan</param>
        /// <param name="plan">The plan request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan updated</returns>
        Task<string> UpdateMarketplacePlanAsync(string offerId,
            string planId,
            string plan,
            LunaRequestHeaders headers);

        /// <summary>
        /// Delete a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="planId">Id of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>Success</returns>
        Task DeleteMarketplacePlanAsync(string offerId,
            string planId,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="planId">Id of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan response content</returns>
        Task<string> GetMarketplacePlanAsync(string offerId,
            string planId,
            LunaRequestHeaders headers);

        /// <summary>
        /// List plans in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plans</returns>
        Task<string> ListMarketplacePlansAsync(string offerId,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create a parameter in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="param">The parameter request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The parameter created</returns>
        Task<string> CreateOfferParameterAsync(string offerId,
            string paramName,
            string param,
            LunaRequestHeaders headers);

        /// <summary>
        /// Update a parameter in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="param">The parameter request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The parameter updated</returns>
        Task<string> UpdateOfferParameterAsync(string offerId,
            string paramName,
            string param,
            LunaRequestHeaders headers);

        /// <summary>
        /// Delete a parameter in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>Success</returns>
        Task DeleteOfferParameterAsync(string offerId,
            string paramName,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get a parameter in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The parameter response content</returns>
        Task<string> GetOfferParameterAsync(string offerId,
            string paramName,
            LunaRequestHeaders headers);

        /// <summary>
        /// List parameters in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer parameters</returns>
        Task<string> ListOfferParametersAsync(string offerId,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="stepName">Name of the provisioning step</param>
        /// <param name="step">The provisioning step request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The provisioning step created</returns>
        Task<string> CreateProvisioningStepAsync(string offerId,
            string stepName,
            string step,
            LunaRequestHeaders headers);

        /// <summary>
        /// Update a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="stepName">Name of the provisioning step</param>
        /// <param name="step">The provisioning step request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The provisioning step updated</returns>
        Task<string> UpdateProvisioningStepAsync(string offerId,
            string stepName,
            string step,
            LunaRequestHeaders headers);

        /// <summary>
        /// Delete a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="stepName">Name of the provisioning step</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>Success</returns>
        Task DeleteProvisioningStepAsync(string offerId,
            string stepName,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="stepName">Name of the provisioning step</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The provisioning step</returns>
        Task<string> GetProvisioningStepAsync(string offerId,
            string stepName,
            LunaRequestHeaders headers);

        /// <summary>
        /// List a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The provisioning step</returns>
        Task<string> ListProvisioningStepsAsync(string offerId,
            LunaRequestHeaders headers);

        #region subscriptions

        /// <summary>
        /// Resolve a marketplace token
        /// </summary>
        /// <param name="token">The subscription token</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The resolved subscription</returns>
        Task<string> ResolveMarketplaceTokenAsync(string token, LunaRequestHeaders headers);

        /// <summary>
        /// Create a markplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="subscription">The subscription</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The created subscription</returns>
        Task<string> CreateMarketplaceSubscriptionAsync(
            Guid subscriptionId,
            string subscription,
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
        Task<string> GetMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers);

        /// <summary>
        /// List marketplace subscriptions
        /// </summary>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        Task<string> ListMarketplaceSubscriptionsAsync(LunaRequestHeaders headers);

        /// <summary>
        /// List marketplace subscription details
        /// </summary>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        Task<string> ListMarketplaceSubscriptionDetailsAsync(LunaRequestHeaders headers);

        /// <summary>
        /// Get parameters for the specified offer and plan
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan id</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The parameters</returns>
        Task<string> GetMarketplaceParametersAsync(string offerId, string planId, LunaRequestHeaders headers);

        #endregion
    }
}
