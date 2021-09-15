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
        /// <param name="name">Name of the offer</param>
        /// <param name="offer">The offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task<MarketplaceOffer> CreateMarketplaceOfferAsync(string name,
            MarketplaceOffer offer,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}");

            var content = JsonConvert.SerializeObject(offer);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, content, headers);

            return await GetResponseObject<MarketplaceOffer>(response);
        }

        /// <summary>
        /// Update an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="offer">The offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task<MarketplaceOffer> UpdateMarketplaceOfferAsync(string name,
            MarketplaceOffer offer,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}");

            var content = JsonConvert.SerializeObject(offer);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, content, headers);

            return await GetResponseObject<MarketplaceOffer>(response);
        }

        /// <summary>
        /// Publish an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task PublishMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}/publish");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, null, headers);

            return;
        }

        /// <summary>
        /// Delete an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task DeleteMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }

        /// Create a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="plan">The plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        public async Task<MarketplacePlan> CreateMarketplacePlanAsync(string offerName,
            string planName,
            MarketplacePlan plan,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName, 
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, 
                nameof(offerName));

            ValidationUtils.ValidateStringValueLength(planName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(planName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans/{planName}");

            var content = JsonConvert.SerializeObject(plan);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, content, headers);

            return await GetResponseObject<MarketplacePlan>(response);
        }

        /// Update a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="plan">The plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        public async Task<MarketplacePlan> UpdateMarketplacePlanAsync(string offerName,
            string planName,
            MarketplacePlan plan,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(offerName));

            ValidationUtils.ValidateStringValueLength(planName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(planName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans/{planName}");

            var content = JsonConvert.SerializeObject(plan);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, content, headers);

            return await GetResponseObject<MarketplacePlan>(response);
        }

        /// Delete a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        public async Task DeleteMarketplacePlanAsync(string offerName,
            string planName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(offerName));

            ValidationUtils.ValidateStringValueLength(planName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(planName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans/{planName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }

        /// <summary>
        /// Get an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        public async Task<JObject> GetMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<JObject>(response);
        }

        /// <summary>
        /// List Azure marketplace offers
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        public async Task<List<MarketplaceOffer>> ListMarketplaceOffersAsync(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<List<MarketplaceOffer>>(response);
        }

        /// Get a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan</returns>
        public async Task<MarketplacePlan> GetMarketplacePlanAsync(string offerName,
            string planName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(offerName));

            ValidationUtils.ValidateStringValueLength(planName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(planName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans/{planName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<MarketplacePlan>(response);
        }

        /// List plans in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plans</returns>
        public async Task<List<MarketplacePlan>> ListMarketplacePlansAsync(string offerName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(offerName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<List<MarketplacePlan>>(response);
        }
        #region subscriptions

        /// <summary>
        /// Resolve a marketplace token
        /// </summary>
        /// <param name="token">The subscription token</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The resolved subscription</returns>
        public async Task<MarketplaceSubscription> ResolveMarketplaceTokenAsync(string token, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"marketplace/subscriptions/resolvetoken");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, token, headers);

            var subscription = JsonConvert.DeserializeObject<MarketplaceSubscription>(await response.Content.ReadAsStringAsync());

            return subscription;
        }

        /// <summary>
        /// Create a markplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="subscription">The subscription</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns>The created subscription</returns>
        public async Task<MarketplaceSubscription> CreateMarketplaceSubscriptionAsync(
            Guid subscriptionId,
            MarketplaceSubscription subscription,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"marketplace/subscriptions/{subscriptionId}");

            var content = JsonConvert.SerializeObject(subscription);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, content, headers);

            var result = JsonConvert.DeserializeObject<MarketplaceSubscription>(await response.Content.ReadAsStringAsync());

            return result;
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
                $"marketplace/subscriptions/{subscriptionId}/activate");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, null, headers);

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
                $"marketplace/subscriptions/{subscriptionId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }

        /// <summary>
        /// Get a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        public async Task<MarketplaceSubscription> GetMarketplaceSubscriptionAsync(Guid subscriptionId, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"marketplace/subscriptions/{subscriptionId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var result = JsonConvert.DeserializeObject<MarketplaceSubscription>(await response.Content.ReadAsStringAsync());

            return result;
        }

        /// <summary>
        /// List marketplace subscriptions
        /// </summary>
        /// <param name="headers">The Luna request header</param>
        /// <returns></returns>
        public async Task<List<MarketplaceSubscription>> ListMarketplaceSubscriptionsAsync(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"marketplace/subscriptions");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var result = JsonConvert.DeserializeObject<List<MarketplaceSubscription>>(await response.Content.ReadAsStringAsync());

            return result;
        }

        /// <summary>
        /// Get parameters for the specified offer and plan
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan id</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer parameters</returns>
        public async Task<List<MarketplaceParameter>> GetMarketplaceParametersAsync(
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
                $"marketplace/offers/{offerId}/plans/{planId}/parameters");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var result = JsonConvert.DeserializeObject<List<MarketplaceParameter>>(await response.Content.ReadAsStringAsync());

            return result;
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
