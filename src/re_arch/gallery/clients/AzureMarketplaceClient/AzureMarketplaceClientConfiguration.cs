using Luna.Common.Utils.RestClients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Clients
{
    public class AzureMarketplaceClientConfiguration : RestClientConfiguration
    {
        public string TenantId { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
