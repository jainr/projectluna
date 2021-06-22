using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Gallery.Data
{
    public class PublishedAzureMarketplacePlanDB
    {
        [Key]
        public long Id { get; set; }

        public string MarketplaceOfferId { get; set; } 

        public string MarketplacePlanId { get; set; }

        public string OfferDisplayName { get; set; }

        public string OfferDescription { get; set; }

        public string Mode { get; set; }

        public string Parameters { get; set; }

        public long LastAppliedEventId { get; set; }

        public bool IsEnabled { get; set; }
    }
}
