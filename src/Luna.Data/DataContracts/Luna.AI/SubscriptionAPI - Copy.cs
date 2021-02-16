using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts.Luna.AI
{
    public class SubscriptionApplication
    {
        public SubscriptionApplication()
        {
            APIs = new List<SubscriptionAPI>();
        }
        public string Name { get; set; }

        public string Description { get; set; }

        public List<SubscriptionAPI> APIs { get; set; }
    }
}
