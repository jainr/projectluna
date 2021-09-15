using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class DeleteMarketplaceOfferEvent : MarketplaceOfferEvent
    {
        public DeleteMarketplaceOfferEvent()
            :base(MarketplaceEventType.DeleteMarketplaceOffer)
        {
        }

        public string Comments { get; set; }
        
    }
}
