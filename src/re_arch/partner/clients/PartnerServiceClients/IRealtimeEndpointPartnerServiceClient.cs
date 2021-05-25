using Luna.Partner.PublicClient.DataContract;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Clients.PartnerServiceClients
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
        void UpdateConfiguration(BasePartnerServiceConfiguration configuration);
    }
}
