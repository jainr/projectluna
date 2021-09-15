using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public abstract class BaseMarketplaceEvent
    {
        public BaseMarketplaceEvent(MarketplaceEventType eventType)
        {
            this.EventType = eventType;
        }

        public string Name { get; set; }

        public MarketplaceEventType EventType { get; }
    }
}
