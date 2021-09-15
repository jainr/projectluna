using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class MarketplacePlanDB
    {
        [Key]
        public long Id { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Mode { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

    }
}
