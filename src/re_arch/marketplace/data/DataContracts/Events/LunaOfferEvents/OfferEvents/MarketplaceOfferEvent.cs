using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public abstract class MarketplaceOfferEvent : BaseMarketplaceEvent
    {
        public MarketplaceOfferEvent(MarketplaceEventType type)
            :base(type)
        {
        }

        public string OfferId { get; set; }
        
    }
}
