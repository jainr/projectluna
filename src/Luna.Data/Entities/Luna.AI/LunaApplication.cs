using Luna.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the offers table in the database.
    /// </summary>
    public partial class LunaApplication
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public LunaApplication()
        {
            IsCreateSaaSOfferAndDefaultPlan = false;
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="service">The object to be copied.</param>
        public void Copy(LunaApplication service)
        {
            this.DisplayName = service.DisplayName;
            this.Owner = service.Owner;
            this.Description = service.Description;
        }

        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        public string DisplayName { get; set; }

        public string ApplicationName { get; set; }

        public string Owner { get; set; }

        public string Description { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        [NotMapped]
        public bool IsCreateSaaSOfferAndDefaultPlan { get; set; }

        [NotMapped]
        public string SaaSOfferName { get; set; }

        [NotMapped]
        public string SaaSOfferPlanName { get; set; }

        [JsonIgnore]
        public virtual ICollection<LunaAPI> Deployments { get; set; }
    }
}