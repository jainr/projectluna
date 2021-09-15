using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class DeleteMarketplaceOfferParameterEvent : MarketplaceOfferParameterEvent
    {
        public DeleteMarketplaceOfferParameterEvent()
            :base(MarketplaceEventType.DeleteMarketplaceOfferParameter)
        {
        }

        public string Comments { get; set; }
        
    }
}
