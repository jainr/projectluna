using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class UpdateMarketplaceOfferFromTemplateEvent : MarketplaceOfferEvent
    {
        public UpdateMarketplaceOfferFromTemplateEvent()
            :base(MarketplaceEventType.UpdateMarketplaceOfferFromTemplate)
        {
        }

        public MarketplaceOffer Offer { get; set; }
        
    }
}
