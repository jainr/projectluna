using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class CreateMarketplaceOfferParameterEvent : MarketplaceOfferParameterEvent
    {
        public CreateMarketplaceOfferParameterEvent()
            :base(MarketplaceEventType.CreateMarketplaceOfferParameter)
        {
        }

        public MarketplaceParameter Parameter { get; set; }
        
    }
}
