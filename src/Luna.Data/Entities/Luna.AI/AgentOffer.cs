using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    [Table("agent_offers")]
    public class AgentOffer
    {
        public AgentOffer()
        {
        }

        [JsonPropertyName("OfferId")]
        public string OfferId { get; set; }

        [JsonPropertyName("OfferName")]
        public string OfferName { get; set; }

        [JsonPropertyName("PublisherId")]
        public Guid PublisherId { get; set; }

        [JsonPropertyName("PublisherMicrosoftId")]
        public string PublisherMicrosoftId { get; set; }

        [JsonPropertyName("PublisherName")]
        public string PublisherName { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("LogoImageUrl")]
        public string LogoImageUrl { get; set; }

        [JsonPropertyName("DocumentationUrl")]
        public string DocumentationUrl { get; set; }

        [JsonPropertyName("LandingPageUrl")]
        public string LandingPageUrl { get; set; }

        [JsonPropertyName("OfferType")]
        public string OfferType { get; set; }

        [JsonPropertyName("LastUpdatedTime")]
        public DateTime LastUpdatedTime { get; set; }

        [JsonPropertyName("CreatedTime")]
        public DateTime CreatedTime { get; set; }

    }
}
