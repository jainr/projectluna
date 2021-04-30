using Luna.Common.Utils.RestClients;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.PublicClient.Clients
{
    public class PartnerServiceClientConfiguration : RestClientConfiguration
    {
        public string ServiceBaseUrl { get; set; }
        public string AuthenticationKey { get; set; }
    }
}
