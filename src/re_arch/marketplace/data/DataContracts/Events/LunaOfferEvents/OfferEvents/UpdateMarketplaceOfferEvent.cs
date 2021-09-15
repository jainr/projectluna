using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class UpdateMarketplaceOfferEvent : MarketplaceOfferEvent
    {
        public UpdateMarketplaceOfferEvent()
            :base(MarketplaceEventType.UpdateMarketplaceOffer)
        {
        }

        public MarketplaceOffer Offer { get; set; }
        
    }
}
