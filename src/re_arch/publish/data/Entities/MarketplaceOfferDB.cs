using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Publish.Data
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

        public MarketplaceOffer ToMarketplaceOffer()
        {
            return new MarketplaceOffer()
            {
                OfferId = this.OfferId,
                Status = this.Status,
                Properties = new MarketplaceOfferProp()
                {
                    DisplayName = this.DisplayName,
                    Description = this.Description
                }
            };
        }

        public void Publish()
        {
            this.Status = MarketplaceOfferStatus.Published.ToString();
            this.LastUpdatedTime = DateTime.UtcNow;
            this.LastPublishedTime = this.LastUpdatedTime;
        }

        public void Delete()
        {
            this.Status = MarketplaceOfferStatus.Deleted.ToString();
            this.LastUpdatedTime = DateTime.UtcNow;
            this.DeletedTime = this.LastUpdatedTime;
        }

        [Key]
        public long Id { get; set; }

        public string OfferId { get; set; }

        public string Status { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? LastPublishedTime { get; set; }

        public DateTime? DeletedTime { get; set; }
    }
}
