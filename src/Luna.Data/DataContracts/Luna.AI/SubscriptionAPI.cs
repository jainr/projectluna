using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts.Luna.AI
{
    public class SubscriptionAPI
    {
        public SubscriptionAPI()
        {
            Versions = new List<SubscriptionAPIVersion>();
        }
        public string Name { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public List<SubscriptionAPIVersion> Versions { get; set; }
    }
}
