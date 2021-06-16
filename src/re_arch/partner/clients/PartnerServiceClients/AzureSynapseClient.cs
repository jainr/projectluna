using Luna.Partner.Public.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.Partner.Clients
{
    /// <summary>
    /// The client class for Azure ML
    /// </summary>
    public class AzureSynapseClient : IPartnerServiceClient
    {
        private AzureSynapseWorkspaceConfiguration _config;

        public AzureSynapseClient(BasePartnerServiceConfiguration configuration)
        {
            this._config = (AzureSynapseWorkspaceConfiguration)configuration;

        }

        /// <summary>
        /// Validate an registered partner service
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            //TODO: implement test connection
            return true;
        }

        /// <summary>
        /// Update the configuration of the service client
        /// </summary>
        /// <param name="configuration">The configuration</param>
        public async Task UpdateConfigurationAsync(BasePartnerServiceConfiguration configuration)
        {
            this._config = (AzureSynapseWorkspaceConfiguration)configuration;
        }
    }
}
