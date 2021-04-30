using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client.DataContracts
{
    public class Ownership
    {
        public string Uid { get; set; }

        public string ResourceId { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
