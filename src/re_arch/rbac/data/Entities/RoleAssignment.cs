using Luna.RBAC.Public.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Data
{
    public class RoleAssignmentDb
    {
        public RoleAssignmentDb()
        {
            this.CreatedTime = DateTime.UtcNow;
        }

        public long Id { get; set; }

        public string Uid { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
