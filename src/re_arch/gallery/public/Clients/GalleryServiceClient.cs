using Luna.Common.Utils;
using Luna.Publish.Public.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Gallery.Public.Client
{
    public class GalleryServiceClient : RestClient, IGalleryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GalleryServiceClient> _logger;
        private readonly GalleryServiceClientConfiguration _config;

        [ActivatorUtilitiesConstructor]
        public GalleryServiceClient(IOptionsMonitor<GalleryServiceClientConfiguration> option,
            HttpClient httpClient,
            ILogger<GalleryServiceClient> logger) :
            base(option, httpClient, logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._config = option.CurrentValue ?? throw new ArgumentNullException(nameof(option.CurrentValue));
        }

        /// <summary>
        /// List all luna applications
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The list of Luna applications</returns>
        public async Task<List<PublishedLunaApplication>> ListLunaApplications(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            List<PublishedLunaApplication> applications =
                JsonConvert.DeserializeObject<List<PublishedLunaApplication>>(
                    await response.Content.ReadAsStringAsync());

            return applications;
        }

        /// <summary>
        /// Get a Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application</returns>
        public async Task<PublishedLunaApplication> GetLunaApplication(string appName, LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var application =
                JsonConvert.DeserializeObject<PublishedLunaApplication>(
                    await response.Content.ReadAsStringAsync());

            return application;
        }


        /// <summary>
        /// Get swagger from a Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application swagger</returns>
        public async Task<object> GetLunaApplicationSwagger(string appName, LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/swagger");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var swagger = JObject.Parse(await response.Content.ReadAsStringAsync());

            return swagger;
        }

        /// <summary>
        /// Get recommended Luna application based on current application
        /// </summary>
        /// <param name="appName">The current application name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The recommended Luna applications</returns>
        public async Task<List<PublishedLunaApplication>> GetRecommendedLunaApplications(
            string appName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/recommended");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            List<PublishedLunaApplication> applications =
                JsonConvert.DeserializeObject<List<PublishedLunaApplication>>(
                    await response.Content.ReadAsStringAsync());

            return applications;
        }

        /// <summary>
        /// Create a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionName">The subscription name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription</returns>
        public async Task<LunaApplicationSubscription> CreateLunaApplicationSubscription(
            string appName,
            string subscriptionName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(subscriptionName, nameof(subscriptionName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/subscriptions/{subscriptionName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, null, headers);

            var sub =
                JsonConvert.DeserializeObject<LunaApplicationSubscription>(
                    await response.Content.ReadAsStringAsync());

            return sub;
        }

        /// <summary>
        /// List subscriptions for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscriptions</returns>
        public async Task<List<LunaApplicationSubscription>> ListLunaApplicationSubscription(
            string appName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/subscriptions");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var subList =
                JsonConvert.DeserializeObject<List<LunaApplicationSubscription>>(
                    await response.Content.ReadAsStringAsync());

            return subList;
        }

        /// <summary>
        /// Get a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription</returns>
        public async Task<LunaApplicationSubscription> GetLunaApplicationSubscription(
            string appName,
            string subscriptionNameOrId,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(subscriptionNameOrId, nameof(subscriptionNameOrId));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/subscriptions/{subscriptionNameOrId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var sub =
                JsonConvert.DeserializeObject<LunaApplicationSubscription>(
                    await response.Content.ReadAsStringAsync());

            return sub;
        }

        /// <summary>
        /// Delete a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns></returns>
        public async Task DeleteLunaApplicationSubscription(
            string appName,
            string subscriptionNameOrId,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(subscriptionNameOrId, nameof(subscriptionNameOrId));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/subscriptions/{subscriptionNameOrId}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }

        /// <summary>
        /// Update notes fpr a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="notes">The subscription notes</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription notes</returns>
        public async Task<LunaApplicationSubscriptionNotes> UpdateLunaApplicationSubscriptionNotes(
            string appName,
            string subscriptionNameOrId,
            string notes,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(subscriptionNameOrId, nameof(subscriptionNameOrId));
            ValidationUtils.ValidateStringValueLength(notes, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(notes));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/subscriptions/{subscriptionNameOrId}/UpdateNotes");

            var content = JsonConvert.SerializeObject(new LunaApplicationSubscriptionNotes()
            {
                Notes = notes
            });

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, content, headers);

            var result =
                JsonConvert.DeserializeObject<LunaApplicationSubscriptionNotes>(
                    await response.Content.ReadAsStringAsync());

            return result;
        }

        /// <summary>
        /// Add a owner a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="userId">The user id for the new owner</param>
        /// <param name="userName">The user name for the new owner</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription owner</returns>
        public async Task<LunaApplicationSubscriptionOwner> AddLunaApplicationSubscriptionOwner(
            string appName,
            string subscriptionNameOrId,
            string userId,
            string userName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(subscriptionNameOrId, nameof(subscriptionNameOrId));
            ValidationUtils.ValidateObjectId(userId, nameof(userId));
            ValidationUtils.ValidateStringValueLength(userName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(userName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/subscriptions/{subscriptionNameOrId}/AddOwner");

            var content = JsonConvert.SerializeObject(new LunaApplicationSubscriptionOwner()
            {
                UserId = userId,
                UserName = userName
            });

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, content, headers);

            var owner =
                JsonConvert.DeserializeObject<LunaApplicationSubscriptionOwner>(
                    await response.Content.ReadAsStringAsync());

            return owner;
        }

        /// <summary>
        /// Remove a owner a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="userId">The user id for the new owner</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription owner</returns>
        public async Task<LunaApplicationSubscriptionOwner> RemoveLunaApplicationSubscriptionOwner(
            string appName,
            string subscriptionNameOrId,
            string userId,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(subscriptionNameOrId, nameof(subscriptionNameOrId));
            ValidationUtils.ValidateObjectId(userId, nameof(userId));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/subscriptions/{subscriptionNameOrId}/RemoveOwner");

            var content = JsonConvert.SerializeObject(new LunaApplicationSubscriptionOwner()
            {
                UserId = userId,
                UserName = string.Empty
            });

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, content, headers);

            var owner =
                JsonConvert.DeserializeObject<LunaApplicationSubscriptionOwner>(
                    await response.Content.ReadAsStringAsync());

            return owner;
        }

        /// <summary>
        /// Regenerate the API key for a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="keyName">The name of the key</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription keys</returns>
        public async Task<LunaApplicationSubscriptionKeys> RegenerateLunaApplicationSubscriptionKey(
            string appName,
            string subscriptionNameOrId,
            string keyName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(subscriptionNameOrId, nameof(subscriptionNameOrId));
            ValidationUtils.ValidateStringInList(keyName, GalleryServiceQueryParametersConstants.GetValidKeyNames(), nameof(keyName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/subscriptions/{subscriptionNameOrId}/RegenerateKey?" +
                $"{GalleryServiceQueryParametersConstants.SUBCRIPTION_KEY_NAME_PARAM_NAME}={keyName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, null, headers);

            var keys =
                JsonConvert.DeserializeObject<LunaApplicationSubscriptionKeys>(
                    await response.Content.ReadAsStringAsync());

            return keys;
        }


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

        /// <summary>
        /// Create a application publisher
        /// </summary>
        /// <param name="name">Name of the application publisher</param>
        /// <param name="publisher">The application publisher</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The publisher created</returns>
        public async Task<ApplicationPublisher> CreateApplicationPublisherAsync(string name,
            ApplicationPublisher publisher,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applicationpublishers/{name}");

            var content = JsonConvert.SerializeObject(publisher);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, content, headers);

            var result = JsonConvert.DeserializeObject<ApplicationPublisher>(await response.Content.ReadAsStringAsync());

            return result;
        }

        /// <summary>
        /// Update a application publisher
        /// </summary>
        /// <param name="name">Name of the application publisher</param>
        /// <param name="publisher">The application publisher</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The publisher updated</returns>
        public async Task<ApplicationPublisher> UpdateApplicationPublisherAsync(string name,
            ApplicationPublisher publisher,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applicationpublishers/{name}");

            var content = JsonConvert.SerializeObject(publisher);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, content, headers);

            var result = JsonConvert.DeserializeObject<ApplicationPublisher>(await response.Content.ReadAsStringAsync());

            return result;
        }

        /// <summary>
        /// Get a application publisher
        /// </summary>
        /// <param name="name">Name of the application publisher</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The publisher</returns>
        public async Task<ApplicationPublisher> GetApplicationPublisherAsync(string name,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applicationpublishers/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var result = JsonConvert.DeserializeObject<ApplicationPublisher>(await response.Content.ReadAsStringAsync());

            return result;
        }

        /// <summary>
        /// List application publishers
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The publishers</returns>
        public async Task<List<ApplicationPublisher>> ListApplicationPublishersAsync(LunaRequestHeaders headers, string type = null)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var requestUrl = this._config.ServiceBaseUrl + $"applicationpublishers";
            if (!string.IsNullOrEmpty(type))
            {
                requestUrl = requestUrl + $"?{GalleryServiceQueryParametersConstants.PUBLISHER_TYPE_PARAM_NAME}={type}";
            }

            var uri = new Uri(requestUrl);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var result = JsonConvert.DeserializeObject<List<ApplicationPublisher>>(await response.Content.ReadAsStringAsync());

            return result;
        }

        /// <summary>
        /// Delete a application publisher
        /// </summary>
        /// <param name="name">Name of the application publisher</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns></returns>
        public async Task DeleteApplicationPublisherAsync(string name,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applicationpublishers/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }
    }
}
