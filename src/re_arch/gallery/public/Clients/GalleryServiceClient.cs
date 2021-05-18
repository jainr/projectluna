using Luna.Common.Utils.RestClients;
using Luna.Gallery.Public.Client.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Gallery.Public.Client.Clients
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
        public async Task<LunaApplicationSwagger> GetLunaApplicationSwagger(string appName, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"applications/{appName}/swagger");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var swagger =
                JsonConvert.DeserializeObject<LunaApplicationSwagger>(
                    await response.Content.ReadAsStringAsync());

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
    }
}
