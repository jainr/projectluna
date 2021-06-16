using Luna.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.RBAC.Public.Client
{
    public class RBACClientConfiguration : RestClientConfiguration
    {
        public string ServiceBaseUrl { get; set; }
        public string AuthenticationKey { get; set; }
    }
}
