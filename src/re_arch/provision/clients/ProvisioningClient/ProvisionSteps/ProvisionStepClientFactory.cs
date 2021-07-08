using Luna.Common.Utils;
using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Clients
{
    public class ProvisionStepClientFactory : IProvisionStepClientFactory
    {
        public IAsyncProvisionStepClient GetAsyncProvisionStepClient(MarketplaceProvisioningStep step)
        {
            if (step.Type.Equals(MarketplaceProvisioningStepType.Script.ToString()))
            {
                var client = new ScriptProvisionStepClient((ScriptProvisioningStepProp)step.Properties);
                return client;
            }
            else
            {
                throw new LunaServerException($"invalid step with type {step.Type}");
            }
        }

        public ISyncProvisionStepClient GetSyncProvisionStepClient(MarketplaceProvisioningStep step)
        {
            throw new NotImplementedException();
        }
    }
}
