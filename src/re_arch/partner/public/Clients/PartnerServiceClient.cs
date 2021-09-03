using Luna.Common.Utils;
using Luna.Publish.Public.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Public.Client
{
    public class PartnerServiceClient : RestClient, IPartnerServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PartnerServiceClient> _logger;
        private readonly PartnerServiceClientConfiguration _config;
        private readonly IEncryptionUtils _encryptionUtils;

        [ActivatorUtilitiesConstructor]
        public PartnerServiceClient(IOptionsMonitor<PartnerServiceClientConfiguration> option,
            HttpClient httpClient,
            IEncryptionUtils encryptionUtils,
            ILogger<PartnerServiceClient> logger) :
            base(option, httpClient, logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._encryptionUtils = encryptionUtils ?? throw new ArgumentNullException(nameof(encryptionUtils));
            this._config = option.CurrentValue ?? throw new ArgumentNullException(nameof(option.CurrentValue));
        }

        /// <summary>
        /// Get the specified partner service
        /// </summary>
        /// <param name="name">The name of the partner service</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The partner service configuration</returns>
        public async Task<BasePartnerServiceConfiguration> GetPartnerServiceConfigurationAsync(string name, LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);
            var content = await response.Content.ReadAsStringAsync();
            var config = JsonConvert.DeserializeObject<BasePartnerServiceConfiguration>(
                    content, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

            if (config.Type.Equals(PartnerServiceType.AzureML.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                config = JsonConvert.DeserializeObject<AzureMLWorkspaceConfiguration>(
                       content, new JsonSerializerSettings()
                       {
                           TypeNameHandling = TypeNameHandling.Auto
                       });
            }
            else if (config.Type.Equals(PartnerServiceType.AzureSynapse.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                config = JsonConvert.DeserializeObject<AzureSynapseWorkspaceConfiguration>(
                       content, new JsonSerializerSettings()
                       {
                           TypeNameHandling = TypeNameHandling.Auto
                       });
            }
            else if (config.Type.Equals(PartnerServiceType.GitHub.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                config = JsonConvert.DeserializeObject<GitHubPartnerServiceConfiguration>(
                       content, new JsonSerializerSettings()
                       {
                           TypeNameHandling = TypeNameHandling.Auto
                       });
            }

            return config;
        }

        /// <summary>
        /// List all partner services
        /// </summary>
        /// <param name="headers">The luna request headers</param>
        /// <returns>All registered partner services</returns>
        public async Task<List<PartnerServiceOutlineResponse>> ListPartnerServicesAsync(LunaRequestHeaders headers, string type = null)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;

            var serviceBaseUrl = this._config.ServiceBaseUrl + "partnerServices";
            if (!string.IsNullOrEmpty(type))
            {
                serviceBaseUrl = serviceBaseUrl + 
                    $"?{PartnerQueryParameterConstats.PARTNER_SERVICE_TYPE_QUERY_PARAM_NAME}={type}";
            }

            var uri = new Uri(serviceBaseUrl);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            List<PartnerServiceOutlineResponse> services =
                JsonConvert.DeserializeObject<List<PartnerServiceOutlineResponse>>(
                    await response.Content.ReadAsStringAsync());

            return services;
        }

        /// <summary>
        /// Register a new partner service
        /// </summary>
        /// <param name="name">The name of partner service</param>
        /// <param name="config">The configuration</param>
        /// <param name="headers">The request headers</param>
        /// <returns></returns>
        public async Task<BasePartnerServiceConfiguration> RegisterPartnerServiceAsync(
            string name,
            BasePartnerServiceConfiguration config,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");

            await config.EncryptSecretsAsync(_encryptionUtils);

            var content = JsonConvert.SerializeObject(config, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, content, headers);

            return config;
        }

        /// <summary>
        /// Update a partner service
        /// </summary>
        /// <param name="name">The name of partner service</param>
        /// <param name="config">The configuration</param>
        /// <param name="headers">The request headers</param>
        /// <returns></returns>
        public async Task<BasePartnerServiceConfiguration> UpdatePartnerServiceAsync(
            string name,
            BasePartnerServiceConfiguration config,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");

            await config.EncryptSecretsAsync(_encryptionUtils);

            var content = JsonConvert.SerializeObject(config, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, content, headers);

            return config;
        }

        /// <summary>
        /// Delete a partner service
        /// </summary>
        /// <param name="name">The name of partner service</param>
        /// <param name="headers">The request headers</param>
        /// <returns></returns>
        public async Task<bool> DeletePartnerServiceAsync(
            string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// List all Azure ML workspaces
        /// </summary>
        /// <param name="headers">The luna request headers</param>
        /// <returns>All registered Azure ML workspaces</returns>
        public async Task<List<PartnerServiceOutlineResponse>> ListAzureMLWorkspaces(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl +
                $"partnerServices?{PartnerQueryParameterConstats.PARTNER_SERVICE_TYPE_QUERY_PARAM_NAME}={PartnerServiceType.AzureML.ToString()}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            List<PartnerServiceOutlineResponse> services =
                JsonConvert.DeserializeObject<List<PartnerServiceOutlineResponse>>(
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
            ValidationUtils.ValidateObjectId(name, nameof(name));

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
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");

            await config.EncryptSecretsAsync(_encryptionUtils);

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
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");

            await config.EncryptSecretsAsync(_encryptionUtils);

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
            ValidationUtils.ValidateObjectId(name, nameof(name));

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
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{name}");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            AzureSynapseWorkspaceConfiguration config =
                JsonConvert.DeserializeObject<AzureSynapseWorkspaceConfiguration>(
                    await response.Content.ReadAsStringAsync());

            return config;
        }

        /// <summary>
        /// Get ML compute service types
        /// </summary>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The compute service types</returns>
        public async Task<List<ServiceType>> GetMLComputeServiceTypes(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/computeservicetypes");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            List<ServiceType> serviceTypes =
                JsonConvert.DeserializeObject<List<ServiceType>>(
                    await response.Content.ReadAsStringAsync());

            return serviceTypes;
        }

        /// <summary>
        /// Get ML host service types
        /// </summary>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The host service types</returns>
        public async Task<List<ServiceType>> GetMLHostServiceTypes(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/hostservicetypes");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            List<ServiceType> serviceTypes =
                JsonConvert.DeserializeObject<List<ServiceType>>(
                    await response.Content.ReadAsStringAsync());

            return serviceTypes;
        }

        /// <summary>
        /// Get ML component types
        /// </summary>
        /// <param name="serviceType">The host service type</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The component types</returns>
        public async Task<List<ComponentType>> GetMLComponentTypes(string serviceType, LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateEnum(serviceType, typeof(PartnerServiceType), nameof(serviceType));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{serviceType}/mlcomponenttypes");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            List<ComponentType> componentTypes =
                JsonConvert.DeserializeObject<List<ComponentType>>(
                    await response.Content.ReadAsStringAsync());

            return componentTypes;
        }

        /// Get specified type of ML components from specified partner service
        /// </summary>
        /// <param name="serviceName">The partner service name</param>
        /// <param name="componentType">The component type</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The ML components</returns>
        public async Task<List<BaseMLComponent>> GetMLComponents(string serviceName, string componentType, LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(serviceName, nameof(serviceName));
            ValidationUtils.ValidateEnum(componentType, typeof(LunaAPIType), nameof(componentType));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"partnerServices/{serviceName}/mlcomponents/{componentType}");
            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            List<BaseMLComponent> components =
                JsonConvert.DeserializeObject<List<BaseMLComponent>>(
                    await response.Content.ReadAsStringAsync());

            return components;
        }
    }
}
