using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    public partial class PlanGateway
    {
        public PlanGateway()
        {

        }

        public long PlanId { get; set; }


        public long GatewayId { get; set; }

        [JsonIgnore]
        public virtual Plan Plan { get; set; }

        [JsonIgnore]
        public virtual Gateway Gateway {get;set;}

    }
}
