using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class BaseMarketplaceOfferEvent
    {
        public BaseMarketplaceOfferEvent(MarketplaceOfferEventType eventType)
        {
            this.EventType = eventType;
        }

        public string Name { get; set; }

        public MarketplaceOfferEventType EventType { get; }
    }
}
