using Luna.Partner.PublicClient.DataContract.PartnerServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.Clients.PartnerServiceClients
{
    /// <summary>
    /// The client interface to access real time endpoints
    /// </summary>
    public interface IPartnerServiceClient
    {
        /// <summary>
        /// Validate an registered partner service
        /// </summary>
        bool TestConnection();

        /// <summary>
        /// Update the configuration of the service client
        /// </summary>
        /// <param name="configuration">The configuration in JSON format</param>
        void UpdateConfiguration(BasePartnerServiceConfiguration configuration);

    }
}
