using Luna.Common.Utils;
using Luna.Partner.Public.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Luna.Partner.Clients
{
    /// <summary>
    /// The client class for Azure ML
    /// </summary>
    public class AzureMLWorkspaceClient : IPartnerServiceClient, IRealtimeEndpointPartnerServiceClient, IPipelineEndpointPartnerServiceClient
    {
        private const string AUTHORIZATION_HEADER = "Authorization";
        private const string BEARER_TOKEN_FORMAT = "Bearer {0}";
        private const string TOKEN_AUTHENTICATION_ENDPOINT = "https://login.microsoftonline.com/";
        private const string AUTHENTICATION_RESOURCE_ID = "https://management.core.windows.net";

        private string _accessToken;
        private AzureMLWorkspaceConfiguration _config;
        private HttpClient _httpClient;

        public AzureMLWorkspaceClient(HttpClient httpClient,
            IEncryptionUtils encryptionUtils, 
            BasePartnerServiceConfiguration configuration)
        {
            this._config = (AzureMLWorkspaceConfiguration)configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Task task = this._config.DecryptSecretsAsync(encryptionUtils);
            task.Wait();

            task = this.RefreshAccessToken();
            task.Wait();
        }

        /// <summary>
        /// Validate an registered partner service
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/modelmanagement/v1.0{1}/services",
                this._config.Region,
                this._config.ResourceId);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Update the configuration of the service client
        /// </summary>
        /// <param name="configuration">The configuration in JSON format</param>
        public async Task UpdateConfigurationAsync(BasePartnerServiceConfiguration configuration)
        {
            this._config = (AzureMLWorkspaceConfiguration)configuration;
        }

        /// <summary>
        /// List realtime endpoints from partner services
        /// </summary>
        /// <returns>The endpoint list</returns>
        public async Task<List<RealtimeEndpoint>> ListRealtimeEndpointsAsync()
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/modelmanagement/v1.0{1}/services",
                this._config.Region,
                this._config.ResourceId);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var endpoints = JsonConvert.DeserializeObject<RealtimeEndpointsResponse>(responseContent);

                var result = new List<RealtimeEndpoint>();
                foreach(var endpoint in endpoints.Value)
                {
                    result.Add(new RealtimeEndpoint(endpoint.Id, endpoint.Name));
                }

                return result;
            }
            else
            {
                throw new LunaServerException(
                    string.Format("Failed to get AML realtime endpoints. Status code: {0}. Error: {1}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync()));
            }
        }

        /// <summary>
        /// List pipeline endpoints from partner services
        /// </summary>
        /// <returns>The endpoint list</returns>
        public async Task<List<PipelineEndpoint>> ListPipelineEndpointsAsync()
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/pipelines/v1.0{1}/pipelines",
                this._config.Region,
                this._config.ResourceId);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            var response = await SendRequestWithRetryAfterTokenRefresh(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var endpoints = JsonConvert.DeserializeObject<List<PipelineEndpointResponse>>(responseContent);

                var result = new List<PipelineEndpoint>();
                foreach (var endpoint in endpoints)
                {
                    result.Add(new PipelineEndpoint(endpoint.Id, endpoint.Name));
                }

                return result;
            }
            else
            {
                throw new LunaServerException(
                    string.Format("Failed to get AML pipeline endpoints. Status code: {0}. Error: {1}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync()));
            }
        }

        private async Task<HttpResponseMessage> SendRequestWithRetryAfterTokenRefresh(HttpRequestMessage request, bool tokenRefreshed = false)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this._accessToken);

            var response = await this._httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized && !tokenRefreshed)
            {
                await RefreshAccessToken();

                // Copy the request and try again
                var req = new HttpRequestMessage()
                {
                    Method = request.Method,
                    RequestUri = request.RequestUri,
                    Content = request.Content,
                };

                foreach (var header in request.Headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }

                return await SendRequestWithRetryAfterTokenRefresh(req, true);
            }

            return response;
        }

        private async Task RefreshAccessToken()
        {
            try
            {
                var key = this._config.ClientSecret;
                var credential = new ClientCredential(this._config.ClientId, key);
                var authContext = new AuthenticationContext(TOKEN_AUTHENTICATION_ENDPOINT + this._config.TenantId, false);
                var token = await authContext.AcquireTokenAsync(AUTHENTICATION_RESOURCE_ID, credential);
                this._accessToken = token.AccessToken;
            }
            catch (AdalServiceException e)
            {
                // If unauthorized or bad request, throw user exception. Otherwise rethrow the same exception
                if (e.StatusCode == (int)HttpStatusCode.BadRequest)
                {
                    throw new LunaBadRequestUserException(ErrorMessages.INVALID_TENANT_OR_CLIENT_ID, UserErrorCode.InvalidInput);
                }
                if (e.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    throw new LunaBadRequestUserException(ErrorMessages.INVALID_CLIENT_SECRET, UserErrorCode.InvalidInput);
                }

                throw e;
            }
        }
    }

    public class RealtimeEndpointResponse
    {
        public string Name { get; set; }

        public string Id { get; set; }
    }

    public class RealtimeEndpointsResponse
    {
        public List<RealtimeEndpointResponse> Value { get; set; }
    }

    public class PipelineEndpointResponse
    {
        public string Name { get; set; }

        public string Id { get; set; }
    }

    public class PipelineEndpointsResponse
    {
        public List<PipelineEndpointResponse> Value { get; set; }
    }
}
