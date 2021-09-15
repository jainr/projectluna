using Luna.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Clients
{
    public class AzureMarketplaceSaaSClientConfiguration : RestClientConfiguration
    {
        public string TenantId { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
