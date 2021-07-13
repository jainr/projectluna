using Luna.Common.Utils;
using Luna.Publish.Public.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Clients
{
    public class ProvisionStepClientFactory : IProvisionStepClientFactory
    {
        private ILogger _logger;

        public ProvisionStepClientFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IAsyncProvisionStepClient GetAsyncProvisionStepClient(MarketplaceProvisioningStep step)
        {
            if (step.Type.Equals(MarketplaceProvisioningStepType.Script.ToString()))
            {
                var client = new ScriptProvisionStepClient((ScriptProvisioningStepProp)step.Properties, this._logger);
                return client;
            }
            if (step.Type.Equals(MarketplaceProvisioningStepType.ARMTemplate.ToString()))
            {
                var client = new ARMTemplateProvisionStepClient((ARMTemplateProvisioningStepProp)step.Properties, this._logger);
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
