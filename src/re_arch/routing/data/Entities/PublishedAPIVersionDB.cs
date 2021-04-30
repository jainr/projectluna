using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Routing.Data.Entities
{
    /// <summary>
    /// The database entity for published API version
    /// </summary>
    public class PublishedAPIVersionDB
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string ApplicationName { get; set; }

        public string APIName { get; set; }

        public string APIType { get; set; }

        public string VersionName { get; set; }

        public string VersionType { get; set; }

        public string VersionProperties { get; set; }

        public long LastAppliedEventId { get; set; } 

        public string PrimaryMasterKeySecretName { get; set; }

        public string SecondaryMasterKeySecretName { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
