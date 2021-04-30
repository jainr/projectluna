using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.PublicClient.DataContract.PartnerServices
{
    /// <summary>
    /// Base class for all Azure Partner Services
    /// </summary>
    public abstract class AzurePartnerServiceConfiguration : BasePartnerServiceConfiguration
    {
        public AzurePartnerServiceConfiguration(PartnerServiceType type) : 
            base(type)
        {

        }

        public string ResourceId { get; set; }

        public string TenantId { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
