﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client.DataContracts
{
    public class RBACQueryResult
    {
        public RBACQuery Query { get; set; }

        public bool CanAccess { get; set; }

        public string Role { get; set; }
    }
}