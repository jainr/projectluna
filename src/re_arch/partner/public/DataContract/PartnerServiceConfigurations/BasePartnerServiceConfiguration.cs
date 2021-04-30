using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.PublicClient.DataContract.PartnerServices
{
    /// <summary>
    /// Base class for all Partner Services
    /// </summary>
    public class BasePartnerServiceConfiguration
    {
        public BasePartnerServiceConfiguration(PartnerServiceType type)
        {
            this.Type = type.ToString();
        }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public string Tags { get; set; }
    }
}
