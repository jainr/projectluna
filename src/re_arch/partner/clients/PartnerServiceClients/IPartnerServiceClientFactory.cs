using Luna.Partner.Data.Entities;
using Luna.Partner.PublicClient.DataContract;
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
        /// <param name="service">The partner service</param>
        /// <returns>The partner service client</returns>
        IPartnerServiceClient GetPartnerServiceClient(PartnerService service);
    }
}
