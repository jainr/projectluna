using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class PublishMarketplaceOfferEvent : BaseMarketplaceOfferEvent
    {
        public PublishMarketplaceOfferEvent()
            :base(MarketplaceOfferEventType.PublishMarketplaceOffer)
        {
        }

        public string Comments { get; set; }
        
    }
}
