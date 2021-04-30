using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Routing.Data.Entities
{
    public class PartnerServiceDbView
    {

        [JsonIgnore]
        public long Id { get; set; }

        public string UniqueName { get; set; }

        public string Type { get; set; }

        public string ConfigurationSecretName { get; set; }
    }
}
