using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Luna.Common.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceServiceClient : RestClient, IMarketplaceServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MarketplaceServiceClient> _logger;
        private readonly MarketplaceServiceClientConfiguration _config;

        [ActivatorUtilitiesConstructor]
        public MarketplaceServiceClient(IOptionsMonitor<MarketplaceServiceClientConfiguration> option,
            HttpClient httpClient,
            ILogger<MarketplaceServiceClient> logger) :
            base(option, httpClient, logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._config = option.CurrentValue ?? throw new ArgumentNullException(nameof(option.CurrentValue));
        }

        /// <summary>
        /// Create or update Azure marketplace offer from template
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="template">The offer template</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task<MarketplaceOffer> CreateOrUpdateMarketplaceOfferFromTemplateAsync(string name,
            string template,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}/template");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, template, headers);

            return await GetResponseObject<MarketplaceOffer>(response);
        }

        /// <summary>
        /// Create an Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="offer">The offer request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task<string> CreateMarketplaceOfferAsync(string offerId,
            string offer,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, offer, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Update an Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="offer">The offer request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task<string> UpdateMarketplaceOfferAsync(string offerId,
            string offer,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, offer, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Publish an Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task PublishMarketplaceOfferAsync(string offerId,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/publish");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, null, headers);
            return;
        }

        /// <summary>
        /// Delete an Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns></returns>
        public async Task DeleteMarketplaceOfferAsync(string offerId,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }

        /// <summary>
        /// Get an Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        public async Task<string> GetMarketplaceOfferAsync(string offerId,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// List Azure marketplace offers
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        public async Task<string> ListMarketplaceOffersAsync(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Create a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="planId">Id of the plan</param>
        /// <param name="plan">The plan request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        public async Task<string> CreateMarketplacePlanAsync(string offerId,
            string planId,
            string plan,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/plans/{planId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, plan, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Update a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="planId">Id of the plan</param>
        /// <param name="plan">The plan request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan updated</returns>
        public async Task<string> UpdateMarketplacePlanAsync(string offerId,
            string planId,
            string plan,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/plans/{planId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, plan, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Delete a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="planId">Id of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>Success</returns>
        public async Task DeleteMarketplacePlanAsync(string offerId,
            string planId,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/plans/{planId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);
            return;
        }

        /// <summary>
        /// Get a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="planId">Id of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan response content</returns>
        public async Task<string> GetMarketplacePlanAsync(string offerId,
            string planId,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/plans/{planId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// List plans in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plans</returns>
        public async Task<string> ListMarketplacePlansAsync(string offerId,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/plans");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Create a parameter in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="param">The parameter request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The parameter created</returns>
        public async Task<string> CreateOfferParameterAsync(string offerId,
            string paramName,
            string param,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/parameters/{paramName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, param, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Update a parameter in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="param">The parameter request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The parameter updated</returns>
        public async Task<string> UpdateOfferParameterAsync(string offerId,
            string paramName,
            string param,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/parameters/{paramName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, param, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Delete a parameter in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>Success</returns>
        public async Task DeleteOfferParameterAsync(string offerId,
            string paramName,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/parameters/{paramName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);
            return;
        }

        /// <summary>
        /// Get a parameter in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The parameter response content</returns>
        public async Task<string> GetOfferParameterAsync(string offerId,
            string paramName,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/parameters/{paramName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// List parameters in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer parameters</returns>
        public async Task<string> ListOfferParametersAsync(string offerId,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/parameters");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Create a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="stepName">Name of the provisioning step</param>
        /// <param name="step">The provisioning step request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The provisioning step created</returns>
        public async Task<string> CreateProvisioningStepAsync(string offerId,
            string stepName,
            string step,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/provisioningsteps/{stepName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, step, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Update a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="stepName">Name of the provisioning step</param>
        /// <param name="step">The provisioning step request content</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The provisioning step updated</returns>
        public async Task<string> UpdateProvisioningStepAsync(string offerId,
            string stepName,
            string step,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/provisioningsteps/{stepName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, step, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Delete a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="stepName">Name of the provisioning step</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>Success</returns>
        public async Task DeleteProvisioningStepAsync(string offerId,
            string stepName,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/provisioningsteps/{stepName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);
            return;
        }

        /// <summary>
        /// Get a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="stepName">Name of the provisioning step</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The provisioning step</returns>
        public async Task<string> GetProvisioningStepAsync(string offerId,
            string stepName,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/provisioningsteps/{stepName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// List a provisioning step in Azure marketplace offer
        /// </summary>
        /// <param name="offerId">Id of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The provisioning step</returns>
        public async Task<string> ListProvisioningStepsAsync(string offerId,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"offers/{offerId}/provisioningsteps");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        #region subscriptions

        /// <summary>
        /// Resolve a marketplace token
        /// </summary>
        /// <param name="token">The subscription token</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The resolved subscription</returns>
        public async Task<string> ResolveMarketplaceTokenAsync(string token, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"subscriptions/resolvetoken");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, token, headers);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Create a markplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="subscription">The subscription</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The created subscription</returns>
        public async Task<string> CreateMarketplaceSubscriptionAsync(
            Guid subscriptionId,
            string subscription,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"subscriptions/{subscriptionId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, subscription, headers);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Activate a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        public async Task ActivateMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"subscriptions/{subscriptionId}/activate");

            await SendRequestAndVerifySuccess(HttpMethod.Post, uri, null, headers);

            return;
        }

        /// <summary>
        /// Unsubscribe a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        public async Task UnsubscribeMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"subscriptions/{subscriptionId}");

            await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }

        /// <summary>
        /// Get a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        public async Task<string> GetMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"subscriptions/{subscriptionId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// List marketplace subscriptions
        /// </summary>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        public async Task<string> ListMarketplaceSubscriptionsAsync(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"subscriptions");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Get parameters for the specified offer and plan
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan id</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer parameters</returns>
        public async Task<string> GetMarketplaceParametersAsync(
            string offerId,
            string planId,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerId,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(offerId));

            ValidationUtils.ValidateStringValueLength(offerId,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(planId));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"offers/{offerId}/plans/{planId}/parameters");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await response.Content.ReadAsStringAsync();
        }
        #endregion
        
        private async Task<T> GetResponseObject<T>(HttpResponseMessage response, TypeNameHandling typeNameHandling = TypeNameHandling.Auto)
        {
            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings()
            {
                TypeNameHandling = typeNameHandling
            });

            return obj;
        }
    }
}
