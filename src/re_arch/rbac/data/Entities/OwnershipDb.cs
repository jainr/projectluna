using Luna.RBAC.Public.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Data
{
    public class OwnershipDb
    {
        public OwnershipDb()
        {
            CreatedTime = DateTime.UtcNow;
        }

        public long Id { get; set; }

        public string Uid { get; set; }

        public string ResourceId { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
