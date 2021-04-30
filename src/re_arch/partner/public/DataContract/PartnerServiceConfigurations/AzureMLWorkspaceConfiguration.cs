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
        public AzureMLWorkspaceConfiguration() :
            base(PartnerServiceType.AML)
        {

        }

        public string Region { get; set; }
    }
}
