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
    public partial class AIServicePlan
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public AIServicePlan()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="servicePlan">The object to be copied.</param>
        public void Copy(AIServicePlan servicePlan)
        {
            this.AIServicePlanDisplayName = servicePlan.AIServicePlanDisplayName;
            this.Description = servicePlan.Description;
        }

        public bool IsModelPlanType()
        {
            return PlanType.Equals(AIServicePlanTypes.Model.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        public bool IsEndpointPlanType()
        {
            return PlanType.Equals(AIServicePlanTypes.Endpoint.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        public bool IsPipelinePlanType()
        {
            return PlanType.Equals(AIServicePlanTypes.Pipeline.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        public bool IsMLProjectPlanType()
        {
            return PlanType.Equals(AIServicePlanTypes.MLProject.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        public long AIServiceId { get; set; }

        [NotMapped]
        public string AIServiceName { get; set; }

        public string AIServicePlanName { get; set; }

        public string AIServicePlanDisplayName { get; set; }

        public string Description { get; set; }

        public string PlanType { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        [JsonIgnore]
        public virtual AIService AIService { get; set; }

        [JsonIgnore]
        public virtual ICollection<APIVersion> Versions { get; set; }
    }
}