using Luna.Partner.Data;
using Luna.Partner.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Clients
{
    public interface IPartnerServiceClientFactory
    {
        /// <summary>
        /// Get or create a partner service client
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        Task<IPartnerServiceClient> GetPartnerServiceClientAsync(string name, BasePartnerServiceConfiguration config);

        /// <summary>
        /// Get or create a partner service client for realtime endpoints
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        Task<IRealtimeEndpointPartnerServiceClient> GetRealtimeEndpointPartnerServiceClientAsync(string name, BasePartnerServiceConfiguration config);

        /// <summary>
        /// Get or create a partner service client for pipeline endpoints
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        Task<IPipelineEndpointPartnerServiceClient> GetPipelineEndpointPartnerServiceClientAsync(string name, BasePartnerServiceConfiguration config);
    }
}
