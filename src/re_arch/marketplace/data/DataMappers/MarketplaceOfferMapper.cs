using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Marketplace.Data
{

    public class MarketplaceOfferMapper :
        IDataMapper<MarketplaceOfferRequest, MarketplaceOfferResponse, MarketplaceOfferProp>
    {

        public MarketplaceOfferProp Map(MarketplaceOfferRequest request)
        {
            MarketplaceOfferProp prop = new MarketplaceOfferProp
            {
                DisplayName = request.DisplayName,
                Description = request.Description,
                IsManualActivation = request.IsManualActivation
            };

            return prop;
        }

        public MarketplaceOfferResponse Map(MarketplaceOfferProp prop)
        {
            MarketplaceOfferResponse response = new MarketplaceOfferResponse
            {
                DisplayName = prop.DisplayName,
                Description = prop.Description,
                IsManualActivation = prop.IsManualActivation
            };

            return response;
        }
    }
}
