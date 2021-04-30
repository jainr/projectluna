using Luna.Common.Utils.RestClients;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Luna.Publish.PublicClient.DataContract.APIVersions;
using Luna.Routing.Clients.MLServiceClients.Interfaces;
using Luna.Routing.Data.DataContracts;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients.MLServiceClients
{
    public class AzureMLClient : IRealtimeEndpointClient, IPipelineEndpointClient
    {
        private const string AUTHORIZATION_HEADER = "Authorization";
        private const string BEARER_TOKEN_FORMAT = "Bearer {0}";
        private const string TOKEN_AUTHENTICATION_ENDPOINT = "https://login.microsoftonline.com/";
        private const string AUTHENTICATION_RESOURCE_ID = "https://management.core.windows.net";

        private readonly AzureMLWorkspaceConfiguration _config;
        private readonly AzureMLCache _cache;
        private readonly HttpClient _httpClient;
        private string _accessToken;

        public AzureMLClient(HttpClient httpClient, AzureMLWorkspaceConfiguration config)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._config = config;
            this._cache = new AzureMLCache();
            Task task = this.RefreshAccessToken();
            task.Wait();
        }

        /// <summary>
        /// Call the realtime endpoint
        /// </summary>
        /// <param name="operationName">The operation name</param>
        /// <param name="input">The input in JSON format</param>
        /// <param name="headers">The headers</param>
        /// <returns>The response</returns>
        public async Task<HttpResponseMessage> CallRealtimeEndpoint(
            string operationName,
            string input,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers)
        {
            return await CallRealtimeEndpointInternal(operationName, input, versionProperties, headers);
        }

        /// <summary>
        /// Call the realtime endpoint
        /// </summary>
        /// <param name="operationName">The operation name</param>
        /// <param name="input">The input in JSON format</param>
        /// <param name="headers">The headers</param>
        /// <returns>The response</returns>
        private async Task<HttpResponseMessage> CallRealtimeEndpointInternal(
            string operationName,
            string input,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers,
            bool shouldRefreshCacheAndRetry = true)
        {
            AzureMLRealtimeEndpointAPIVersionProp prop = (AzureMLRealtimeEndpointAPIVersionProp)versionProperties;
            var endpointName = prop.Endpoints.Where(x => x.OperationName == operationName).SingleOrDefault().EndpointName;

            var endpoint = await GetRealtimeEndpoint(endpointName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint.Url);
            request.Content = new StringContent(input);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            if (endpoint.AuthEnabled && !endpoint.AadAuthEnabled)
            {
                request.Headers.Add(AUTHORIZATION_HEADER, string.Format(BEARER_TOKEN_FORMAT, endpoint.Key));
            }
            else if (endpoint.AadAuthEnabled)
            {
                request.Headers.Add(AUTHORIZATION_HEADER, string.Format(BEARER_TOKEN_FORMAT, this._accessToken));
            }

            headers.AddToHttpRequestHeaders(request.Headers);

            var response = await this._httpClient.SendAsync(request);

            if (shouldRefreshCacheAndRetry && 
                (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Unauthorized))
            {
                return await CallRealtimeEndpointInternal(operationName, input, versionProperties, headers, false);
            }

            return response;
        }

        /// <summary>
        /// Execute pipeline by calling the pipeline endpoint
        /// </summary>
        /// <param name="input">the input in JSON format</param>
        /// <param name="headers">The headers</param>
        /// <param name="predecessorOperationId">The predecessor operation id if specified</param>
        /// <returns>The operation id</returns>
        public async Task<string> ExecutePipeline(string input, LunaRequestHeaders headers, string predecessorOperationId = null)
        {
            return "";
        }

        /// <summary>
        /// Get the pipeline execution status
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="headers">The headers</param>
        /// <returns>The operation status</returns>
        public async Task<OperationStatus> GetPipelineExecutionStatus(string operationId, LunaRequestHeaders headers)
        {
            return null;
        }

        /// <summary>
        /// Get the pipeline execution output in Json format
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="headers">The headers</param>
        /// <returns>The execution output in Json format</returns>
        public async Task<string> GetPipelineExecutionJsonOutput(string operationId, LunaRequestHeaders headers)
        {
            return "";
        }

        private async Task RefreshAccessToken()
        {
            var key = this._config.ClientSecret;
            var credential = new ClientCredential(this._config.ClientId, key);
            var authContext = new AuthenticationContext(TOKEN_AUTHENTICATION_ENDPOINT + this._config.TenantId, false);
            var token = await authContext.AcquireTokenAsync(AUTHENTICATION_RESOURCE_ID, credential);
            this._accessToken = token.AccessToken;
        }

        private async Task<AzureMLRealtimeEndpointCache> GetRealtimeEndpoint(string endpointName)
        {
            if (!this._cache.RealTimeEndpoints.ContainsKey(endpointName))
            {
                var endpointInfo = await GetRealtimeEndpointInfo(endpointName);

                var endpoint = new AzureMLRealtimeEndpointCache()
                {
                    Url = endpointInfo.ScoringUri,
                    AuthEnabled = endpointInfo.AuthEnabled,
                    AadAuthEnabled = endpointInfo.AadAuthEnabled
                };

                if (endpointInfo.AuthEnabled && !endpointInfo.AadAuthEnabled)
                {
                    endpoint.Key = await GetRealtimeEndpointKey(endpointName);
                }

                _cache.RealTimeEndpoints.Add(endpointName, endpoint);
            }

            return this._cache.RealTimeEndpoints[endpointName];

        }

        private async Task<RealtimeEndpointResponse> GetRealtimeEndpointInfo(string endpointName, bool tokenRefreshed = false)
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/modelmanagement/v1.0{1}/services/{2}",
                this._config.Region,
                this._config.ResourceId,
                endpointName);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var endpoint = JsonConvert.DeserializeObject<RealtimeEndpointResponse>(responseContent);
                return endpoint;
            }

            throw new LunaServerException(
                string.Format("Failed to get AML realtime endpoint. Status code: {0}. Error: {1}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync()));
        }

        private async Task<string> GetRealtimeEndpointKey(string endpointName, bool tokenRefreshed = false)
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/modelmanagement/v1.0{1}/services/{2}/listkeys",
                this._config.Region,
                this._config.ResourceId,
                endpointName);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var endpoint = JsonConvert.DeserializeObject<RealtimeEndpointKeysResponse>(responseContent);
                return endpoint.PrimaryKey;
            }
            else
            {
                throw new LunaServerException(
                    string.Format("Failed to get AML realtime keys. Status code: {0}. Error: {1}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync()));
            }
        }

        private async Task<HttpResponseMessage> SendRequestWithRetryAfterTokenRefresh(HttpRequestMessage request, bool tokenRefreshed = false)
        {
            request.Headers.Add("Authorization", $"Bearer {this._accessToken}");
            var response = await this._httpClient.SendAsync(request);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized && !tokenRefreshed)
            {
                await RefreshAccessToken();
                return await SendRequestWithRetryAfterTokenRefresh(request, true);
            }

            return response;
        }

        private Task<string> GetRealtimeEndpointScoringUrl(string endpointName, bool v)
        {
            throw new NotImplementedException();
        }
    }


    public class RealtimeEndpointResponse
    {
        public string Name { get; set; }
        public string ScoringUri { get; set; }

        public bool AuthEnabled { get; set; }

        public bool AadAuthEnabled { get; set; }
    }

    public class RealtimeEndpointKeysResponse

    {
        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }
    }
}
