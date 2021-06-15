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
        
        public static string example = JsonConvert.SerializeObject(new
        {
            DisplayName = "My Azure ML workspace",
            Description = "Azure ML workspace",
            Type = PartnerServiceType.AzureML.ToString(),
            Tags = ""
        });
       
        //public static string example = "{}";

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
        [JsonProperty(PropertyName = "Tags", Required = Required.Default)]
        public string Tags { get; set; }
    }
}
