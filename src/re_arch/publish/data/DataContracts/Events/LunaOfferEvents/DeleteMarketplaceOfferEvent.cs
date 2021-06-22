using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class DeleteMarketplaceOfferEvent : BaseMarketplaceOfferEvent
    {
        public DeleteMarketplaceOfferEvent()
            :base(MarketplaceOfferEventType.DeleteMarketplaceOffer)
        {
        }

        public string Comments { get; set; }
        
    }
}
