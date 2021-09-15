using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public abstract class ProvisioningStepEvent : BaseMarketplaceEvent
    {
        public ProvisioningStepEvent(MarketplaceEventType type)
            :base(type)
        {
        }

        public string OfferId { get; set; }

        public string StepName { get; set; }
        
    }
}
