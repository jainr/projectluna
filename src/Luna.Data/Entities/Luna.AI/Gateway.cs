using Luna.Data.Entities.Luna.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    public partial class Gateway
    {
        public Gateway()
        {

        }

        public void Copy(Gateway gateway)
        {
            GatewayId = gateway.GatewayId;
            DisplayName = gateway.DisplayName;
            Description = gateway.Description;
            Tags = gateway.Tags;
            IsPrivate = gateway.IsPrivate;
        }

        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        public string Name { get; set; }

        public Guid GatewayId { get; set; }

        public string DisplayName { get; set; }

        public string EndpointUrl { get; set; }

        public string Description { get; set; }

        public string Tags { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public bool IsPrivate { get; set; }

        [JsonIgnore]
        public virtual ICollection<AIServicePlanGateway> AIServicePlanGateways { get; set; }
    }
}
