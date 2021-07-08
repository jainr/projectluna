using Luna.Common.Utils;
using Luna.Partner.Public.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Clients
{
    public class PartnerServiceClientFactory : IPartnerServiceClientFactory
    {
        private static ConcurrentDictionary<string, IPartnerServiceClient> _partnerServiceClient;
        private static ConcurrentDictionary<string, IRealtimeEndpointPartnerServiceClient> _realtimeEndpointPartnerServiceClient;
        private static ConcurrentDictionary<string, IPipelineEndpointPartnerServiceClient> _pipelineEndpointPartnerServiceClient;
        private HttpClient _httpClient;
        private readonly IEncryptionUtils _encryptionUtils;

        public PartnerServiceClientFactory(HttpClient httpClient,
            IEncryptionUtils encryptionUtils)
        {
            _httpClient = httpClient;
            this._encryptionUtils = encryptionUtils ?? throw new ArgumentNullException(nameof(encryptionUtils));
            _partnerServiceClient = new ConcurrentDictionary<string, IPartnerServiceClient>();
            _realtimeEndpointPartnerServiceClient = new ConcurrentDictionary<string, IRealtimeEndpointPartnerServiceClient>();
            _pipelineEndpointPartnerServiceClient = new ConcurrentDictionary<string, IPipelineEndpointPartnerServiceClient>();
        }

        /// <summary>
        /// Get or create a partner service client
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        public async Task<IPartnerServiceClient> GetPartnerServiceClientAsync(string name, BasePartnerServiceConfiguration config)
        {
            if (_partnerServiceClient.ContainsKey(name))
            {
                await _partnerServiceClient[name].UpdateConfigurationAsync(config);
                return _partnerServiceClient[name];
            }

            if (config.Type.Equals(PartnerServiceType.AzureML.ToString(), 
                StringComparison.InvariantCultureIgnoreCase))
            {
                _partnerServiceClient.TryAdd(name, new AzureMLWorkspaceClient(_httpClient, _encryptionUtils, config));
            }
            else if (config.Type.Equals(PartnerServiceType.GitHub.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                _partnerServiceClient.TryAdd(name, new GitHubClient(_httpClient, _encryptionUtils, config));
            }

            return _partnerServiceClient[name];
        }

        /// <summary>
        /// Get or create a partner service client for realtime endpoints
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        public async Task<IRealtimeEndpointPartnerServiceClient> GetRealtimeEndpointPartnerServiceClientAsync(string name, BasePartnerServiceConfiguration config)
        {
            if (_realtimeEndpointPartnerServiceClient.ContainsKey(name))
            {
                await _realtimeEndpointPartnerServiceClient[name].UpdateConfigurationAsync(config);
                return _realtimeEndpointPartnerServiceClient[name];
            }

            if (config.Type.Equals(PartnerServiceType.AzureML.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                _realtimeEndpointPartnerServiceClient.TryAdd(name, 
                    new AzureMLWorkspaceClient(_httpClient, _encryptionUtils, config));
            }

            return _realtimeEndpointPartnerServiceClient[name];
        }

        /// <summary>
        /// Get or create a partner service client for pipeline endpoints
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        public async Task<IPipelineEndpointPartnerServiceClient> GetPipelineEndpointPartnerServiceClientAsync(string name, BasePartnerServiceConfiguration config)
        {
            if (_pipelineEndpointPartnerServiceClient.ContainsKey(name))
            {
                await _pipelineEndpointPartnerServiceClient[name].UpdateConfigurationAsync(config);
                return _pipelineEndpointPartnerServiceClient[name];
            }

            IPipelineEndpointPartnerServiceClient client = null;

            if (config.Type.Equals(PartnerServiceType.AzureML.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                _pipelineEndpointPartnerServiceClient.TryAdd(name, 
                    new AzureMLWorkspaceClient(_httpClient, _encryptionUtils, config));
            }

            return client;
        }
    }
}
