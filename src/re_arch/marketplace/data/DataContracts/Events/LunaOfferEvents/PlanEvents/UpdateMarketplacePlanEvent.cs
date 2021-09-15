using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class UpdateMarketplacePlanEvent : MarketplacePlanEvent
    {
        public UpdateMarketplacePlanEvent()
            : base(MarketplaceEventType.UpdateMarketplacePlan)
        {
        }

        public MarketplacePlan Plan { get; set; }
        
    }
}
