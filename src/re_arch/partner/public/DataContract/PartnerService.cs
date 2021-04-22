using Luna.Partner.PublicClient.DataContract.PartnerServices;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luna.Partner.PublicClient.DataContract
{
    public class PartnerService
    {
        public string UniqueName { get; set; }

        public string DisplayName { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public BasePartnerServiceConfiguration Configuration { get; set; }

        public string Tags { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
