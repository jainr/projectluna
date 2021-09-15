using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class MarketplaceParameterDB
    {
        [Key]
        public long Id { get; set; }

        public string OfferId { get; set; }

        public string ParameterName { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

    }
}
