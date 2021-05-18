using Luna.Publish.PublicClient.DataContract.APIVersions;
using Luna.Publish.PublicClient.DataContract.LunaApplications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Data.Entities
{
    public class LunaApplicationSwaggerDB
    {

        [JsonIgnore]
        public long Id { get; set; }

        public string ApplicationName { get; set; }

        public string DisplayName { get; set; }

        public long LastAppliedEventId { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }


    }
}
