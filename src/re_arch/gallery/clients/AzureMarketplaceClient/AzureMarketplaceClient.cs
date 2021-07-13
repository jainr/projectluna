using Luna.Common.Utils;
using Luna.Gallery.Clients;
using Luna.Gallery.Public.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Gallery.Clients
{
    public class AzureMarketplaceClient : RestClient, IAzureMarketplaceClient
    {
        private const string API_VERSION = "2018-08-31";
        private const string REQUEST_ID_HEADER_NAME = "x-ms-requestid";
        private const string CORRALATION_ID_HEADER_NAME = "x-ms-correlationid";
        private const string MARKETPLACE_TOKEN_HEADER_NAME = "x-ms-marketplace-token";
        private const string TOKEN_AUTHENTICATION_ENDPOINT = "https://login.microsoftonline.com/";
        private const string AUTHENTICATION_RESOURCE_ID = "20e940b3-4c77-4b0b-9a53-9e16a1b010a7";

        private readonly HttpClient _httpClient;
        private readonly ILogger<AzureMarketplaceClient> _logger;
        private readonly AzureMarketplaceClientConfiguration _config;

        [ActivatorUtilitiesConstructor]
        public AzureMarketplaceClient(IOptionsMonitor<AzureMarketplaceClientConfiguration> option,
            HttpClient httpClient,
            ILogger<AzureMarketplaceClient> logger) :
            base(option, httpClient, logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._config = option.CurrentValue ?? throw new ArgumentNullException(nameof(option.CurrentValue));
        }

        /// <summary>
        /// Resolve a marketplace subscription from token
        /// </summary>
        /// <param name="token">The token</param>
        /// <param name="headers">The request headers</param>
        /// <returns>The marketplace subscription</returns>
        public async Task<MarketplaceSubscription> ResolveMarketplaceSubscriptionAsync(
            string token, 
            LunaRequestHeaders headers)
        {
            Uri requestUri = new Uri(@$"https://marketplaceapi.microsoft.com/api/saas/subscriptions/resolve?api-version={API_VERSION}");

            //Remove the quotes if exist
            if (token.StartsWith("\"") && token.EndsWith("\""))
            {
                token = token.Substring(1, token.Length - 2);
            }

            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();
            additionalHeaders.Add(MARKETPLACE_TOKEN_HEADER_NAME, token);

            var response = await SendMarketplaceRequest(HttpMethod.Post, requestUri, null, headers, additionalHeaders);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var resolvedSub = JsonConvert.DeserializeObject<ResolvedMarketplaceSubscriptionResponse>(content);

                if (resolvedSub != null && resolvedSub.Subscription != null)
                {
                    return resolvedSub.ToMarketplaceSubscription();
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new LunaBadRequestUserException(ErrorMessages.INVALID_MARKETPLACE_TOKEN, UserErrorCode.InvalidInput);
            }

            throw new LunaServerException($"Failed to resolve token. StatusCode: {response.StatusCode}");

        }

        /// <summary>
        /// Activate a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="planId">The plan id</param>
        /// <param name="headers">The request header</param>
        /// <returns></returns>
        public async Task ActivateMarketplaceSubscriptionAsync(
            Guid subscriptionId, 
            string planId,
            LunaRequestHeaders headers)
        {
            Uri requestUri = new Uri(
                @$"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}/activate?api-version={API_VERSION}");

            var requestContent = JsonConvert.SerializeObject(new { PlanId = planId });

            var response = await SendMarketplaceRequest(HttpMethod.Post, requestUri, requestContent, headers);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new LunaNotFoundUserException(
                        string.Format(ErrorMessages.MARKETPLACE_SUBSCRIPTION_DOES_NOT_EXIST, subscriptionId));
                }
                else
                {
                    throw new LunaServerException($"Failed to activate the subscription. StatusCode: {response.StatusCode}");
                }
            }

        }

        /// <summary>
        /// Get a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The request header</param>
        /// <returns>The marketplace subscription</returns>
        public async Task<MarketplaceSubscription> GetMarketplaceSubscriptionAsync(
            Guid subscriptionId, 
            LunaRequestHeaders headers)
        {
            Uri requestUri = new Uri(
                @$"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}?api-version={API_VERSION}");

            var response = await SendMarketplaceRequest(HttpMethod.Get, requestUri, null, headers);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var sub = JsonConvert.DeserializeObject<MarketplaceSubscriptionResponse>(content);

                if (sub != null)
                {
                    return sub.ToMarketplaceSubscription();
                }
            }

            throw new LunaServerException($"Failed to get the subscription. StatusCode: {response.StatusCode}");
        }

        /// <summary>
        /// Unsubscribe a marketplace subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription id</param>
        /// <param name="headers">The request header</param>
        /// <returns></returns>
        public async Task UnsubscribeMarketplaceSubscriptionAsync(
            Guid subscriptionId, 
            LunaRequestHeaders headers)
        {
            Uri requestUri = new Uri(
                @$"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}?api-version={API_VERSION}");

            var response = await SendMarketplaceRequest(HttpMethod.Delete, requestUri, null, headers);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new LunaNotFoundUserException(
                        string.Format(ErrorMessages.MARKETPLACE_SUBSCRIPTION_DOES_NOT_EXIST, subscriptionId));
                }
                else
                {
                    throw new LunaServerException($"Failed to activate the subscription. StatusCode: {response.StatusCode}");
                }
            }
        }

        private async Task<HttpResponseMessage> SendMarketplaceRequest(
            HttpMethod method, 
            Uri requestUri, 
            string content, 
            LunaRequestHeaders headers,
            Dictionary<string, string> additionalHeaders = null)
        {
            var request = BuildRequest(method, requestUri, content, headers);
            request.Headers.Add(REQUEST_ID_HEADER_NAME, headers.TraceId);
            request.Headers.Add(CORRALATION_ID_HEADER_NAME, headers.TraceId);

            if (additionalHeaders != null)
            {
                foreach (var key in additionalHeaders.Keys)
                {
                    request.Headers.Add(key, additionalHeaders[key]);
                }
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            var response = await _httpClient.SendAsync(request);

            return response;
        }

        private async Task<string> GetAccessToken()
        {
            var key = this._config.ClientSecret;
            var credential = new ClientCredential(this._config.ClientId, key);
            var authContext = new AuthenticationContext(TOKEN_AUTHENTICATION_ENDPOINT + this._config.TenantId, false);
            var token = await authContext.AcquireTokenAsync(AUTHENTICATION_RESOURCE_ID, credential);
            return token.AccessToken;
        }
    }
}
