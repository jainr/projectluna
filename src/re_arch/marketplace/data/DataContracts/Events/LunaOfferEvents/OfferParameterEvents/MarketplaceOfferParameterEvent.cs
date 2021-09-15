using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public abstract class MarketplaceOfferParameterEvent : BaseMarketplaceEvent
    {
        public MarketplaceOfferParameterEvent(MarketplaceEventType type)
            :base(type)
        {
        }

        public string OfferId { get; set; }

        public string ParameterName { get; set; }
        
    }
}
