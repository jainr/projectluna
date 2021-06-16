using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients
{
    public interface IMLServiceClientFactory
    {
        /// <summary>
        /// Get the realtime endpoint client
        /// </summary>
        /// <param name="versionType">The api version type</param>
        /// <param name="versionProperties">The api version properties</param>
        /// <returns>The realtime endpoint client</returns>
        Task<IRealtimeEndpointClient> GetRealtimeEndpointClient(string versionType, BaseAPIVersionProp versionProperties);

        /// <summary>
        /// Get the pipeline endpoint client
        /// </summary>
        /// <param name="versionType">The api version type</param>
        /// <param name="versionProperties">The api version properties</param>
        /// <returns>The pipeline endpoint client</returns>
        Task<IPipelineEndpointClient> GetPipelineEndpointClient(string versionType, BaseAPIVersionProp versionProperties);
    }
}
