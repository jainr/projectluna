using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Partner.PublicClient.DataContract.PartnerServices
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
            Type = PartnerServiceType.AML.ToString(),
            Tags = "purpose=prod;org=hr",
            ResourceId = @"/subscriptions/" + 
                Guid.NewGuid().ToString() +
                "/resourceGroups/rg-name/providers/Microsoft.MachineLearningServices/workspaces/workspace-name",
            TenantId = Guid.NewGuid().ToString(),
            ClientId = Guid.NewGuid().ToString(),
            ClientSecret = "my-client-secret",
            Region="westus"

        });

        public BasePartnerServiceConfiguration(PartnerServiceType type)
        {
            this.Type = type.ToString();
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
