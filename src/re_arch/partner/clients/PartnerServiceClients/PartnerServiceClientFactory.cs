using Luna.Common.Utils;
using Luna.Partner.PublicClient.DataContract;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Luna.Partner.Clients.PartnerServiceClients
{
    public class PartnerServiceClientFactory : IPartnerServiceClientFactory
    {
        private static Dictionary<string, IPartnerServiceClient> _partnerServiceClient;
        private static Dictionary<string, IRealtimeEndpointPartnerServiceClient> _realtimeEndpointPartnerServiceClient;
        private static Dictionary<string, IPipelineEndpointPartnerServiceClient> _pipelineEndpointPartnerServiceClient;
        private HttpClient _httpClient;
        private readonly IEncryptionUtils _encryptionUtils;

        public PartnerServiceClientFactory(HttpClient httpClient,
            IEncryptionUtils encryptionUtils)
        {
            _httpClient = httpClient;
            this._encryptionUtils = encryptionUtils ?? throw new ArgumentNullException(nameof(encryptionUtils));
            _partnerServiceClient = new Dictionary<string, IPartnerServiceClient>();
            _realtimeEndpointPartnerServiceClient = new Dictionary<string, IRealtimeEndpointPartnerServiceClient>();
            _pipelineEndpointPartnerServiceClient = new Dictionary<string, IPipelineEndpointPartnerServiceClient>();
        }

        /// <summary>
        /// Get or create a partner service client
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        public IPartnerServiceClient GetPartnerServiceClient(string name, BasePartnerServiceConfiguration config)
        {
            if (_partnerServiceClient.ContainsKey(name))
            {
                _partnerServiceClient[name].UpdateConfiguration(config);
                return _partnerServiceClient[name];
            }

            IPartnerServiceClient client = null;

            if (config.Type.Equals(PartnerServiceType.AzureML.ToString(), 
                StringComparison.InvariantCultureIgnoreCase))
            {
                client = new AzureMLWorkspaceClient(_httpClient, _encryptionUtils, config);
            }

            if (client != null)
            {
                _partnerServiceClient.TryAdd(name, client);
            }

            return client;
        }

        /// <summary>
        /// Get or create a partner service client for realtime endpoints
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        public IRealtimeEndpointPartnerServiceClient GetRealtimeEndpointPartnerServiceClient(string name, BasePartnerServiceConfiguration config)
        {
            if (_realtimeEndpointPartnerServiceClient.ContainsKey(name))
            {
                _realtimeEndpointPartnerServiceClient[name].UpdateConfiguration(config);
                return _realtimeEndpointPartnerServiceClient[name];
            }

            IRealtimeEndpointPartnerServiceClient client = null;

            if (config.Type.Equals(PartnerServiceType.AzureML.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                client = new AzureMLWorkspaceClient(_httpClient, _encryptionUtils, config);
            }

            if (client != null)
            {
                _realtimeEndpointPartnerServiceClient.TryAdd(name, client);
            }

            return client;
        }

        /// <summary>
        /// Get or create a partner service client for pipeline endpoints
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        public IPipelineEndpointPartnerServiceClient GetPipelineEndpointPartnerServiceClient(string name, BasePartnerServiceConfiguration config)
        {
            if (_pipelineEndpointPartnerServiceClient.ContainsKey(name))
            {
                _pipelineEndpointPartnerServiceClient[name].UpdateConfiguration(config);
                return _pipelineEndpointPartnerServiceClient[name];
            }

            IPipelineEndpointPartnerServiceClient client = null;

            if (config.Type.Equals(PartnerServiceType.AzureML.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                client = new AzureMLWorkspaceClient(_httpClient, _encryptionUtils, config);
            }

            if (client != null)
            {
                _pipelineEndpointPartnerServiceClient.TryAdd(name, client);
            }

            return client;
        }
    }
}
