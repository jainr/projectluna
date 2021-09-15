using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class CreateMarketplacePlanEvent : MarketplacePlanEvent
    {
        public CreateMarketplacePlanEvent()
            :base(MarketplaceEventType.CreateMarketplacePlan)
        {
        }

        public MarketplacePlan Plan { get; set; }
        
    }
}
