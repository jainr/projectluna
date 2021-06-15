using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.Public.Client
{
    /// <summary>
    /// The database entity for Azure Synapse workspace
    /// </summary>
    public class AzureSynapseWorkspaceConfiguration : AzurePartnerServiceConfiguration
    {
        public AzureSynapseWorkspaceConfiguration() :
            base(PartnerServiceType.AzureSynapse)
        {

        }
    }
}
