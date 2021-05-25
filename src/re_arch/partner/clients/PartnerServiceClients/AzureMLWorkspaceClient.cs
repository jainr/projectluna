using Luna.Partner.PublicClient.DataContract;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.Partner.Clients.PartnerServiceClients
{
    /// <summary>
    /// The client class for Azure ML
    /// </summary>
    public class AzureMLWorkspaceClient : IPartnerServiceClient, IRealtimeEndpointPartnerServiceClient, IPipelineEndpointPartnerServiceClient
    {
        private AzureMLWorkspaceConfiguration _config;
        public AzureMLWorkspaceClient(BasePartnerServiceConfiguration configuration)
        {
            this._config = (AzureMLWorkspaceConfiguration)configuration;
        }

        /// <summary>
        /// Validate an registered partner service
        /// </summary>
        public bool TestConnection()
        {
            //TODO: implement test connection
            return true;
        }

        /// <summary>
        /// Update the configuration of the service client
        /// </summary>
        /// <param name="configuration">The configuration in JSON format</param>
        public void UpdateConfiguration(BasePartnerServiceConfiguration configuration)
        {
            this._config = (AzureMLWorkspaceConfiguration)configuration;
        }

        /// <summary>
        /// List realtime endpoints from partner services
        /// </summary>
        /// <returns>The endpoint list</returns>
        public async Task<List<RealtimeEndpoint>> ListRealtimeEndpointsAsync()
        {
            return new List<RealtimeEndpoint>() { };
        }

        /// <summary>
        /// List pipeline endpoints from partner services
        /// </summary>
        /// <returns>The endpoint list</returns>
        public async Task<List<PipelineEndpoint>> ListPipelineEndpointsAsync()
        {
            return new List<PipelineEndpoint>() { };
        }
    }
}
