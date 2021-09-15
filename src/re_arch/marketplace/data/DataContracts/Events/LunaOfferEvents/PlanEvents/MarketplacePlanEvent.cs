using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public abstract class MarketplacePlanEvent : BaseMarketplaceEvent
    {
        public MarketplacePlanEvent(MarketplaceEventType type)
            :base(type)
        {
        }

        public string OfferId { get; set; }
        public string PlanId { get; set; }
                
    }
}
