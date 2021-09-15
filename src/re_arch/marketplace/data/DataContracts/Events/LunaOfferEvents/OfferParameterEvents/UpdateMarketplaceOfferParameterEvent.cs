using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class UpdateMarketplaceOfferParameterEvent : MarketplaceOfferParameterEvent
    {
        public UpdateMarketplaceOfferParameterEvent()
            :base(MarketplaceEventType.CreateMarketplaceOfferParameter)
        {
        }

        public MarketplaceParameter Parameter { get; set; }
        
    }
}
