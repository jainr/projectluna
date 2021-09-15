using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class CreateMarketplaceOfferEvent : MarketplaceOfferEvent
    {
        public CreateMarketplaceOfferEvent()
            :base(MarketplaceEventType.CreateMarketplaceOffer)
        {
        }

        public MarketplaceOffer Offer { get; set; }
        
    }
}
