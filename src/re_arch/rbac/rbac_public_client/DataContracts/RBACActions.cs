﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Public.Client.DataContracts
{
    public class RBACActions
    {
        public const string CREATE_NEW_APPLICATION = @"\applications\create";
        public const string LIST_APPLICATIONS = @"\applications\list";

        public static string[] PublisherAllowedActions = new string[] { CREATE_NEW_APPLICATION, LIST_APPLICATIONS };
    }
}