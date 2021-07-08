using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Clients
{
    public interface IProvisionStepClientFactory
    {
        IAsyncProvisionStepClient GetAsyncProvisionStepClient(MarketplaceProvisioningStep step);

        ISyncProvisionStepClient GetSyncProvisionStepClient(MarketplaceProvisioningStep step);
    }
}
