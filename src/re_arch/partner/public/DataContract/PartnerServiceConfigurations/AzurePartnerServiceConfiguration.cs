using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        public async Task EncryptSecretsAsync(IEncryptionUtils utils)
        {
            this.ClientSecret = await utils.EncryptStringWithSymmetricKeyAsync(this.ClientSecret);
        }
        public async Task DecryptSecretsAsync(IEncryptionUtils utils)
        {
            this.ClientSecret = await utils.DecryptStringWithSymmetricKeyAsync(this.ClientSecret);
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
