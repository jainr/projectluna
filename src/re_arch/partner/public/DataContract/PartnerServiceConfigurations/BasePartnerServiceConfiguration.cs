using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Public.Client
{
    /// <summary>
    /// Base class for all Partner Services
    /// </summary>
    public class BasePartnerServiceConfiguration
    {
        public static string example = JsonConvert.SerializeObject(new AzureMLWorkspaceConfiguration()
        {
            DisplayName = "My Azure ML workspace",
            Description = "Azure ML workspace",
            Type = PartnerServiceType.AzureML.ToString(),
            Tags = "purpose=prod;org=hr",
            ResourceId = @"/subscriptions/" + 
                Guid.NewGuid().ToString() +
                "/resourceGroups/rg-name/providers/Microsoft.MachineLearningServices/workspaces/workspace-name",
            TenantId = "0e2c5f5c-f79f-41b6-b1fe-5e5da2ad10e5",
            ClientId = "75835406-4afa-4b0e-8423-f09315bcf125",
            ClientSecret = "my-client-secret",
            Region="westus"

        });

        public BasePartnerServiceConfiguration(PartnerServiceType type)
        {
            this.Type = type.ToString();
        }

        public virtual async Task EncryptSecretsAsync(IEncryptionUtils utils)
        {
        }

        public virtual async Task DecryptSecretsAsync(IEncryptionUtils utils)
        {
        }

        /// <summary>
        /// The display name of the partner service
        /// </summary>
        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        /// <summary>
        /// The description of the partner service
        /// </summary>
        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        /// <summary>
        /// The type of the partner service
        /// </summary>
        /// <description>The type of the partner service</description>
        /// <example>AzureML</example>
        [JsonProperty(PropertyName = "Type", Required = Required.Always)]
        public string Type { get; set; }

        /// <summary>
        /// The tags for the partner service
        /// </summary>
        [JsonProperty(PropertyName = "Tags", Required = Required.Always)]
        public string Tags { get; set; }
    }
}
