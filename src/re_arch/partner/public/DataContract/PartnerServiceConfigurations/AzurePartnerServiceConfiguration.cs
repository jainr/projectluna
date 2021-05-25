using Newtonsoft.Json;
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

        [JsonProperty(PropertyName = "ResourceId", Required = Required.Always)]
        public string ResourceId { get; set; }

        [JsonProperty(PropertyName = "TenantId", Required = Required.Always)]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "ClientId", Required = Required.Always)]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "ClientSecret", Required = Required.Always)]
        public string ClientSecret { get; set; }
    }
}
