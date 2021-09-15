using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class CreateMarketplaceOfferFromTemplateEvent : MarketplaceOfferEvent
    {
        public CreateMarketplaceOfferFromTemplateEvent()
            :base(MarketplaceEventType.CreateMarketplaceOfferFromTemplate)
        {
        }

        public MarketplaceOffer Offer { get; set; }
        
    }
}
