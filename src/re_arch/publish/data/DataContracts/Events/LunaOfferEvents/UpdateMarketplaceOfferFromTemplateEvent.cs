using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class UpdateMarketplaceOfferFromTemplateEvent : BaseMarketplaceOfferEvent
    {
        public UpdateMarketplaceOfferFromTemplateEvent()
            :base(MarketplaceOfferEventType.UpdateMarketplaceOfferFromTemplate)
        {
        }

        public MarketplaceOffer Offer { get; set; }
        
    }
}
