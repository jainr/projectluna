using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Luna.Data.DataContracts.Luna.AI
{
    public class AIMarketplacePlan
    {
        public AIMarketplacePlan()
        {
        }

        [JsonPropertyName("PlanName")]
        public string PlanName { get; set; }

        [JsonPropertyName("PlanDisplayName")]
        public string PlanDisplayName { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }
    }
}
