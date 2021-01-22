using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Azure;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Clients.Models.Provisioning;
using Luna.Data.DataContracts.Luna.AI;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luna.Clients
{
    /// <summary>
    /// An HTTP client for deploying resources with ARM templates and the resource manager API.
    /// </summary>
    public class AMLClient : RestClient<AMLClient>, IAMLClient
    {
        private readonly IKeyVaultHelper _keyVaultHelper;
        SecuredProvisioningClientConfiguration _options;
        private readonly IOptionsMonitor<AzureConfigurationOption> _azureOptions;

        [ActivatorUtilitiesConstructor]
        public AMLClient(
            IOptionsMonitor<SecuredProvisioningClientConfiguration> optionsMonitor,
            IOptionsMonitor<AzureConfigurationOption> azureOptions,
            ILogger<AMLClient> logger,
            IKeyVaultHelper keyVaultHelper,
            HttpClient httpClient) : base(
            optionsMonitor.CurrentValue,
            logger,
            httpClient)
        {
            _options = optionsMonitor.CurrentValue;
            _azureOptions = azureOptions;
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
        }

        private AuthenticationConfiguration GetAuthenticationConfiguration(AMLWorkspace workspace)
        {
            var config = new AuthenticationConfiguration();
            config.AppKey = workspace.AADApplicationSecretName;
            config.TenantId = workspace.AADTenantId;
            config.ClientId = workspace.AADApplicationId;
            config.VaultName = _azureOptions.CurrentValue.Config.VaultName;
            return config;
        }

        private async Task<HttpResponseMessage> GetAMLResponse(Uri requestUrl, AMLWorkspace workspace)
        {
            var bearerToken = await _keyVaultHelper.GetBearerToken(
                GetAuthenticationConfiguration(workspace),
                _options.ClientService.AuthenticationResourceId);

            var response = await SendRequest(
                HttpMethod.Get,
                requestUrl,
                Guid.NewGuid(),
                Guid.NewGuid(),
                bearerToken,
                null,
                null,
                CancellationToken.None
            );

            return response;
        }

        public async Task<List<MLEndpointArtifact>> GetEndpoints(AMLWorkspace workspace)
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/modelmanagement/v1.0{1}/services",
                workspace.Region,
                workspace.ResourceId);
            var requestUrl = new Uri(url);
            var response = await GetAMLResponse(requestUrl, workspace);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<MLEndpointArtifact>();
            }
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var rawEndpointList = JsonConvert.DeserializeObject<AMLArtifactsResponse>(responseContent);
                List<MLEndpointArtifact> endpointList = new List<MLEndpointArtifact>();
                foreach (var item in rawEndpointList.value)
                {
                    endpointList.Add(new MLEndpointArtifact()
                    {
                        Name = item["name"].ToString()
                    });
                }

                return endpointList;
            }

            throw new LunaProvisioningException(
                $"Request failed to get endpoints",
                ExceptionUtils.IsHttpErrorCodeRetryable(response.StatusCode));
        }

        /// <summary>
        /// Get all compute cluster from an AML workspace
        /// </summary>
        /// <returns></returns>
        public async Task<List<AMLComputeCluster>> GetComputeClusters(AMLWorkspace workspace)
        {
            var url = string.Format(@"https://management.azure.com{1}/computes?api-version=2019-05-01",
                workspace.Region,
                workspace.ResourceId);
            var requestUrl = new Uri(url);
            var response = await GetAMLResponse(requestUrl, workspace);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<AMLComputeCluster>();
            }
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var rawClusterList = JsonConvert.DeserializeObject<AMLArtifactsResponse>(responseContent);
                List<AMLComputeCluster> computeClusterList = new List<AMLComputeCluster>();
                foreach (var item in rawClusterList.value)
                {
                    Dictionary<string, object> properties = ((JObject)item["properties"]).ToObject<Dictionary<string, object>>();
                    if (properties["computeType"].ToString().Equals("AmlCompute", StringComparison.InvariantCultureIgnoreCase))
                    {
                        computeClusterList.Add(new AMLComputeCluster()
                        {
                            Name = item["name"].ToString()
                        });
                    }
                }

                return computeClusterList;
            }

            throw new LunaProvisioningException(
                $"Request failed to get compute clusters",
                ExceptionUtils.IsHttpErrorCodeRetryable(response.StatusCode));
        }

        public async Task<List<MLModelArtifact>> GetModels(AMLWorkspace workspace)
        {
            var url = string.Format(@"https://{0}.api.azureml.ms/modelregistry/v1.0{1}/models",
                workspace.Region,
                workspace.ResourceId);
            var requestUrl = new Uri(url);
            var response = await GetAMLResponse(requestUrl, workspace);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<MLModelArtifact>();
            }
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var rawModelList = JsonConvert.DeserializeObject<AMLArtifactsResponse>(responseContent);
                List<MLModelArtifact> modelList = new List<MLModelArtifact>();
                foreach (var item in rawModelList.value)
                {
                    modelList.Add(new MLModelArtifact()
                    {
                        Name = item["name"].ToString(),
                        Framework = item["framework"].ToString(),
                        Version = item["version"].ToString(),
                        FrameworkVersion = item["frameworkVersion"] != null? item["frameworkVersion"].ToString():"Unknown"
                    });
                }

                return modelList;
            }

            throw new LunaProvisioningException(
                $"Request failed to get models",
                ExceptionUtils.IsHttpErrorCodeRetryable(response.StatusCode));
        }
    }
}
