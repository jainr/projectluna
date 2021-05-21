using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.Entities
{
    /// <summary>
    /// The database entity for application snapshots
    /// </summary>
    public class ApplicationSnapshotDB
    {
        [JsonIgnore]
        public long Id { get; set; }

        public Guid SnapshotId { get; set; }

        public long LastAppliedEventId { get; set; }

        public string ApplicationName { get; set; }

        public string SnapshotContent { get; set; }

        public string Status { get; set; }

        public string Tags { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime? DeletedTime { get; set; }
    }
}
