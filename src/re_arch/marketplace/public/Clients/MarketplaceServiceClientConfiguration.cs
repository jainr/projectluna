using Luna.Common.Utils;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceServiceClientConfiguration : RestClientConfiguration
    {
        public string ServiceBaseUrl { get; set; }
        public string AuthenticationKey { get; set; }
    }
}
