using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Marketplace.Data
{
    public class MarketplaceOfferDB
    {
        public MarketplaceOfferDB()
        {

        }

        public MarketplaceOfferDB(string offerId, string displayName, string description)
        {
            this.OfferId = offerId;
            this.DisplayName = displayName;
            this.Description = description;
            this.Status = MarketplaceOfferStatus.Draft.ToString();
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        [Key]
        public long Id { get; set; }

        public string OfferId { get; set; }

        public string Status { get; set; }

        public string DisplayName { get; set; }

        public string CreatedBy { get; set; }

        public string Description { get; set; }

        public long? LastPublishedEventId { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? LastPublishedTime { get; set; }

        public DateTime? DeletedTime { get; set; }
    }
}
