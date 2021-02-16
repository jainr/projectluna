using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    public partial class PlanApplication
    {
        public PlanApplication()
        {

        }

        public long PlanId { get; set; }


        public long ApplicationId { get; set; }

        [JsonIgnore]
        public virtual Plan Plan { get; set; }

        [JsonIgnore]
        public virtual LunaApplication Application {get;set;}

    }
}
