using Luna.Common.Utils.RestClients;
using Luna.Partner.PublicClient.DataContract;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.PublicClient.Clients
{
    public class PartnerServiceClient : RestClient, IPartnerServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PartnerServiceClient> _logger;
        private readonly PartnerServiceClientConfiguration _config;

        [ActivatorUtilitiesConstructor]
        public PartnerServiceClient(IOptionsMonitor<PartnerServiceClientConfiguration> option,
            HttpClient httpClient,
            ILogger<PartnerServiceClient> logger) : 
            base(option, httpClient, logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._config = option.CurrentValue ?? throw new ArgumentNullException(nameof(option.CurrentValue));
        }

        /// <summary>
        /// List all Azure ML workspaces
        /// </summary>
        /// <param name="headers">The luna request headers</param>
        /// <returns>All registered Azure ML workspaces</returns>
        public async Task<List<PartnerService>> ListAzureMLWorkspaces(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + 
                $"partnerServices?{PartnerQueryParameterConstats.PARTNER_SERVICE_TYPE_QUERY_PARAM_NAME}={PartnerServiceType.AML.ToString()}");
            
            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            List<PartnerService> services =
                JsonConvert.DeserializeObject<List<PartnerService>>(
                    await response.Content.ReadAsStringAsync());

            return services;
        }

        /// <summary>
        /// Get Azure ML workspace configuration
        /// </summary>
        /// <param name="name">The name of the workspace</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The workspace configuration</returns>
        public async Task<AzureMLWorkspaceConfiguration> GetAzureMLWorkspaceConfiguration(string name, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            AzureMLWorkspaceConfiguration config = 
                JsonConvert.DeserializeObject<AzureMLWorkspaceConfiguration>(
                    await response.Content.ReadAsStringAsync());

            return config;
        }

        /// <summary>
        /// Register a new AML workspace as partner service
        /// </summary>
        /// <param name="name">The name of partner service</param>
        /// <param name="config">The configuration</param>
        /// <param name="headers">The request headers</param>
        /// <returns></returns>
        public async Task<AzureMLWorkspaceConfiguration> RegisterAzureMLWorkspace(
            string name,
            AzureMLWorkspaceConfiguration config,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");
            var content = JsonConvert.SerializeObject(config, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, content, headers);

            return config;
        }

        /// <summary>
        /// Update an AML workspace as partner service
        /// </summary>
        /// <param name="name">The name of partner service</param>
        /// <param name="config">The configuration</param>
        /// <param name="headers">The request headers</param>
        /// <returns></returns>
        public async Task<AzureMLWorkspaceConfiguration> UpdateAzureMLWorkspace(
            string name,
            AzureMLWorkspaceConfiguration config,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");
            var content = JsonConvert.SerializeObject(config, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, content, headers);

            return config;
        }

        /// <summary>
        /// Delete an AML workspace as partner service
        /// </summary>
        /// <param name="name">The name of partner service</param>
        /// <param name="headers">The request headers</param>
        /// <returns></returns>
        public async Task<bool> DeleteAzureMLWorkspace(
            string name,
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get Azure Synapse workspace configuration
        /// </summary>
        /// <param name="name">The name of the workspace</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The workspace configuration</returns>
        public async Task<AzureSynapseWorkspaceConfiguration> GetAzureSynapseWorkspaceConfiguration(string name, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            AzureSynapseWorkspaceConfiguration config =
                JsonConvert.DeserializeObject<AzureSynapseWorkspaceConfiguration>(
                    await response.Content.ReadAsStringAsync());

            return config;
        }
    }
}
