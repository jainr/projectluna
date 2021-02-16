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
    public partial class AMLPipelineEndpoint
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public AMLPipelineEndpoint()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="pipeline">The object to be copied.</param>
        public void Copy(AMLPipelineEndpoint pipeline)
        {

        }
    
        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        public long APIVersionId { get; set; }

        public string PipelineEndpointName { get; set; }

        public Guid PipelineEndpointId { get; set; }

    }
}