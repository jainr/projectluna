using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.PublicClient.DataContract.PartnerServices
{
    /// <summary>
    /// The database entity for Azure ML workspace
    /// </summary>
    public class AzureMLWorkspaceConfiguration : AzurePartnerServiceConfiguration
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
            TenantId = Guid.NewGuid().ToString(),
            ClientId = Guid.NewGuid().ToString(),
            ClientSecret = "my-client-secret",
            Region = "westus"
        });

        public AzureMLWorkspaceConfiguration() :
            base(PartnerServiceType.AzureML)
        {

        }

        [JsonProperty(PropertyName = "Region", Required = Required.Always)]
        public string Region { get; set; }
    }
}
