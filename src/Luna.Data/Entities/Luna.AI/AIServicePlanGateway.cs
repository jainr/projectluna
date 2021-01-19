using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    public partial class AIServicePlanGateway
    {
        public AIServicePlanGateway()
        {

        }

        public long AIServicePlanId { get; set; }

        public AIServicePlan AIServicePlan { get; set; }

        public long GatewayId { get; set; }

        public Gateway Gateway {get;set;}

    }
}
