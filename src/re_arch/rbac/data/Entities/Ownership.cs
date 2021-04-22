using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Data.Entities
{
    public class Ownership
    {
        public Ownership()
        {

        }

        [JsonIgnore]
        public long Id { get; set; }

        public string Uid { get; set; }

        public string ResourceId { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
