﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.Azure.AzureKeyvaultUtils
{
    public class SecretNamePrefixes
    {
        public const string PARTNER_SERVICE_CONFIG = "psc-";

        public const string APPLICATION_MASTER_KEY = "amk-";

        public const string SUBSCRIPTION_KEY = "sub-";

        public static string GetNamePrefix(string name)
        {
            if (name.Contains("-"))
            {
                return name.Substring(0, name.IndexOf("-") + 1);
            }

            return string.Empty;
        }
    }
}