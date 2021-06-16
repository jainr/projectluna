using Luna.Common.Utils;

namespace Luna.Publish.Public.Client
{
    public class PublishServiceClientConfiguration : RestClientConfiguration
    {
        public string ServiceBaseUrl { get; set; }
        public string AuthenticationKey { get; set; }
    }
}
