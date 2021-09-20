using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using Luna.Provision.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Test
{
    public class MockProvisionStepClientFactory : IProvisionStepClientFactory
    {
        private ILogger _logger;

        public MockProvisionStepClientFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IAsyncProvisionStepClient GetAsyncProvisionStepClient(MarketplaceProvisioningStep step)
        {
            throw new NotImplementedException();
        }

        public ISyncProvisionStepClient GetSyncProvisionStepClient(MarketplaceProvisioningStep step)
        {
            if (step.Type.Equals(MarketplaceProvisioningStepType.Webhook.ToString()))
            {
                var client = new MockWebhookProvisionStepClient((WebhookProvisioningStepProp)step.Properties);
                return client;
            }
            else
            {
                throw new LunaServerException($"invalid step with type {step.Type}");
            }
        }
    }
}
