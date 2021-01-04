using Luna.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the offers table in the database.
    /// </summary>
    public partial class AIService
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public AIService()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="service">The object to be copied.</param>
        public void Copy(AIService service)
        {
            this.DisplayName = service.DisplayName;
            this.Owner = service.Owner;
            this.LogoImageUrl = service.LogoImageUrl;
            this.Description = service.Description;
            this.DocumentationUrl = service.DocumentationUrl;
            this.Tags = service.Tags;
        }

        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        public string DisplayName { get; set; }

        public string AIServiceName { get; set; }

        public string Owner { get; set; }

        public string LogoImageUrl { get; set; }

        public string Description { get; set; }

        public string DocumentationUrl { get; set; }

        public string Tags { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public string SaaSOfferName { get; set; }

        [JsonIgnore]
        public virtual ICollection<AIServicePlan> Deployments { get; set; }
    }
}