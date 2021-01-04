using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts.Luna.AI
{
    public class ServiceEndpoint
    {
        public string Name { get; set; }
        public string ComputeType  { get; set; }
        public string ModelId { get; set; }
        public string AuthType { get; set; }
    }
}