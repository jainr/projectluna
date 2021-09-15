using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Data
{
    /// <summary>
    /// The database entity for marketplace offer snapshots
    /// </summary>
    public class MarketplaceOfferSnapshotDB
    {
        [JsonIgnore]
        public long Id { get; set; }

        public Guid SnapshotId { get; set; }

        public long LastAppliedEventId { get; set; }

        public string OfferId { get; set; }

        public string SnapshotContent { get; set; }

        public string Status { get; set; }

        public string Tags { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime? DeletedTime { get; set; }
    }
}
