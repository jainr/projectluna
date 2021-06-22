using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class CreateMarketplaceOfferFromTemplateEvent : BaseMarketplaceOfferEvent
    {
        public CreateMarketplaceOfferFromTemplateEvent()
            :base(MarketplaceOfferEventType.CreateMarketplaceOfferFromTemplate)
        {
        }

        public MarketplaceOffer Offer { get; set; }
        
    }
}
