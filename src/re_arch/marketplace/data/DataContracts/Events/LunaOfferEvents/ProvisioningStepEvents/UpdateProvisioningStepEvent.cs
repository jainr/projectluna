using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class UpdateProvisioningStepEvent : ProvisioningStepEvent
    {
        public UpdateProvisioningStepEvent()
            :base(MarketplaceEventType.UpdateMarketplaceProvisioningStep)
        {
        }

        public MarketplaceProvisioningStep Step { get; set; }
        
    }
}
