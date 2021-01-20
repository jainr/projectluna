using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Luna.Data.DataContracts.Luna.AI
{
    public class AIMarketplaceOffer
    {
        public AIMarketplaceOffer()
        {
            OfferType = "internal";
            Plans = new List<AIMarketplacePlan>();
        }
        [JsonPropertyName("OfferName")]
        public string OfferName { get; set; }

        [JsonPropertyName("OfferDisplayName")]
        public string OfferDisplayName { get; set; }

        [JsonPropertyName("Plans")]
        public List<AIMarketplacePlan> Plans { get; set; }

        [JsonPropertyName("PublisherName")]
        public string PublisherName { get; set; }

        [JsonPropertyName("OfferType")]
        public string OfferType { get; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("LogoImageUrl")]
        public string LogoImageUrl { get; set; }

        [JsonPropertyName("DocumentationUrl")]
        public string DocumentationUrl { get; set; }

        [JsonPropertyName("SubscribePageUrl")]
        public string SubscribePageUrl { get; set; }

    }
}
