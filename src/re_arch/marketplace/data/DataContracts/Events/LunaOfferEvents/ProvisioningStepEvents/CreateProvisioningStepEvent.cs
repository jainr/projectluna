using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class CreateProvisioningStepEvent : ProvisioningStepEvent
    {
        public CreateProvisioningStepEvent()
            :base(MarketplaceEventType.CreateMarketplaceProvisioningStep)
        {
        }

        public string StepSecretName { get; set; }

    }
}
