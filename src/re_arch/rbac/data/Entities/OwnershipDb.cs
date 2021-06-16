using Luna.RBAC.Public.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Data
{
    public class OwnershipDb : Ownership
    {
        public OwnershipDb()
        {

        }

        [JsonIgnore]
        public long Id { get; set; }
    }
}
