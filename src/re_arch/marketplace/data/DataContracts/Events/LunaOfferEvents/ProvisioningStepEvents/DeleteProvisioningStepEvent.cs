using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class DeleteProvisioningStepEvent : ProvisioningStepEvent
    {
        public DeleteProvisioningStepEvent()
            :base(MarketplaceEventType.DeleteMarketplaceProvisioningStep)
        {
        }

        public string Comments { get; set; }
        
    }
}
