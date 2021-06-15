using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.Public.Client
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
                "fcd14a2a-31d3-49c1-9e1b-39063da1ac6a" +
                "/resourceGroups/rg-name/providers/Microsoft.MachineLearningServices/workspaces/workspace-name",
            TenantId = "43ebb35e-be1a-4dbf-92da-fb8a069d6a2c",
            ClientId = "114b34b1-4f9a-4888-926a-b222d3b36ef6",
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
