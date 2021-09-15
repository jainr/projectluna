using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    public enum MarketplaceEventType
    {
        CreateMarketplaceOfferFromTemplate,
        UpdateMarketplaceOfferFromTemplate,
        CreateMarketplaceOffer,
        UpdateMarketplaceOffer,
        DeleteMarketplaceOffer,
        PublishMarketplaceOffer,
        CreateMarketplacePlan,
        UpdateMarketplacePlan,
        DeleteMarketplacePlan,
        CreateMarketplaceOfferParameter,
        UpdateMarketplaceOfferParameter,
        DeleteMarketplaceOfferParameter,
        CreateMarketplaceProvisioningStep,
        UpdateMarketplaceProvisioningStep,
        DeleteMarketplaceProvisioningStep
    }
}
