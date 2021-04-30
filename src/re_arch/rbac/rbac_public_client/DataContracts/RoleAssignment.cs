using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client.DataContracts
{
    public class RoleAssignment
    {
        public string Uid { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
