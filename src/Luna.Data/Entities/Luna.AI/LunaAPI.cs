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
    public partial class LunaAPI
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public LunaAPI()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="servicePlan">The object to be copied.</param>
        public void Copy(LunaAPI servicePlan)
        {
            this.APIDisplayName = servicePlan.APIDisplayName;
            this.Description = servicePlan.Description;
            
        }

        public bool IsModelPlanType()
        {
            return APIType.Equals(AIServicePlanTypes.Model.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        public bool IsEndpointPlanType()
        {
            return APIType.Equals(AIServicePlanTypes.Endpoint.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        public bool IsPipelinePlanType()
        {
            return APIType.Equals(AIServicePlanTypes.Pipeline.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        public bool IsMLProjectPlanType()
        {
            return APIType.Equals(AIServicePlanTypes.MLProject.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        public bool IsDatasetPlanType()
        {
            return APIType.Equals(AIServicePlanTypes.Dataset.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        public long ApplicationId { get; set; }

        [NotMapped]
        public string ApplicationName { get; set; }

        public string APIName { get; set; }

        public string APIDisplayName { get; set; }

        public string Description { get; set; }

        public string APIType { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        [JsonIgnore]
        public virtual LunaApplication Application { get; set; }

        [JsonIgnore]
        public virtual ICollection<APIVersion> Versions { get; set; }

    }
}