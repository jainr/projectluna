using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class PublishMarketplaceOfferEvent : MarketplaceOfferEvent
    {
        public PublishMarketplaceOfferEvent()
            :base(MarketplaceEventType.PublishMarketplaceOffer)
        {
        }

        public string Comments { get; set; }
        
    }
}
