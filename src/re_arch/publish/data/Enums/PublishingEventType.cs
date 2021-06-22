using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public enum LunaAppEventType
    {
        CreateLunaApplication,
        UpdateLunaApplication,
        DeleteLunaApplication,
        PublishLunaApplication,
        CreateLunaAPI,
        UpdateLunaAPI,
        DeleteLunaAPI,
        CreateLunaAPIVersion,
        UpdateLunaAPIVersion,
        DeleteLunaAPIVersion
    }

    public enum MarketplaceOfferEventType
    {
        CreateMarketplaceOfferFromTemplate,
        UpdateMarketplaceOfferFromTemplate,
        DeleteMarketplaceOffer,
        PublishMarketplaceOffer
    }
}
