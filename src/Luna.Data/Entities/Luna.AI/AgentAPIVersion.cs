using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    [Table("agent_apiversions")]
    public class AgentAPIVersion
    {
        public AgentAPIVersion()
        {
        }

        [JsonPropertyName("VersionName")]
        public string VersionName { get; set; }

        [JsonPropertyName("DeploymentName")]
        public string DeploymentName { get; set; }

        [JsonPropertyName("ProductName")]
        public string ProductName { get; set; }

        [JsonPropertyName("VersionSourceType")]
        public string VersionSourceType { get; set; }

        [JsonIgnore]
        [JsonPropertyName("ProjectFileUrl")]
        public string ProjectFileUrl { get; set; }

        [JsonPropertyName("CreatedTime")]
        public DateTime CreatedTime { get; set; }

        [JsonPropertyName("LastUpdatedTime")]
        public DateTime LastUpdatedTime { get; set; }

        [JsonIgnore]
        [JsonPropertyName("SubscriptionId")]
        public Guid SubscriptionId { get; set; }

        [JsonIgnore]
        [JsonPropertyName("AgentId")]
        public Guid AgentId { get; set; }

        [JsonPropertyName("PublisherId")]
        public Guid PublisherId { get; set; }

    }
}
