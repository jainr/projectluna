using Luna.Partner.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Clients
{
    public interface IRealtimeEndpointPartnerServiceClient
    {
        /// <summary>
        /// List realtime endpoints from partner services
        /// </summary>
        /// <returns>The endpoint list</returns>
        Task<List<RealtimeEndpoint>> ListRealtimeEndpointsAsync();

        /// <summary>
        /// Update the configuration of the service client
        /// </summary>
        /// <param name="configuration">The configuration in JSON format</param>
        Task UpdateConfigurationAsync(BasePartnerServiceConfiguration configuration);
    }
}
