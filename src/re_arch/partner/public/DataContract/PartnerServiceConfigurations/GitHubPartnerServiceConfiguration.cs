using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Public.Client
{
    /// <summary>
    /// Base class for all Azure Partner Services
    /// </summary>
    public class GitHubPartnerServiceConfiguration : BasePartnerServiceConfiguration
    {
        public GitHubPartnerServiceConfiguration() : 
            base(PartnerServiceType.GitHub)
        {

        }

        public override async Task EncryptSecretsAsync(IEncryptionUtils utils)
        {
            this.PersonalAccessToken = await utils.EncryptStringWithSymmetricKeyAsync(this.PersonalAccessToken);
        }
        public override async Task DecryptSecretsAsync(IEncryptionUtils utils)
        {
            this.PersonalAccessToken = await utils.DecryptStringWithSymmetricKeyAsync(this.PersonalAccessToken);
        }

        [JsonProperty(PropertyName = "HttpsUrl", Required = Required.Always)]
        public string HttpsUrl { get; set; }

        [JsonProperty(PropertyName = "PersonalAccessToken", Required = Required.Always)]
        public string PersonalAccessToken { get; set; }

        [JsonProperty(PropertyName = "DefaultBranch", Required = Required.Always)]
        public string DefaultBranch { get; set; }

    }
}
