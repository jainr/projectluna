using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class DeleteMarketplacePlanEvent : MarketplacePlanEvent
    {
        public DeleteMarketplacePlanEvent()
            :base(MarketplaceEventType.DeleteMarketplacePlan)
        {
        }

        public string Comments { get; set; }
        
    }
}
