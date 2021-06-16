using Luna.Common.Utils;
using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients
{
    public interface IRealtimeEndpointClient
    {
        bool NeedRefresh { get; }

        /// <summary>
        /// Call the realtime endpoint
        /// </summary>
        /// <param name="operationName">The operation name</param>
        /// <param name="input">The input in JSON format</param>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <returns>The response </returns>
        Task<HttpResponseMessage> CallRealtimeEndpoint(
            string operationName, 
            string input, 
            BaseAPIVersionProp versionProperties, 
            LunaRequestHeaders headers);
    }
}
