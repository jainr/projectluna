using Luna.Partner.Data.Entities;
using Luna.Partner.PublicClient.DataContract;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.Clients.PartnerServiceClients
{
    public interface IPartnerServiceClientFactory
    {
        /// <summary>
        /// Get or create a partner service client
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        IPartnerServiceClient GetPartnerServiceClient(string name, BasePartnerServiceConfiguration config);
    }
}
