using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts.Luna.AI
{
    public class SubscriptionAPIVersion
    {
        public SubscriptionAPIVersion()
        {
            Operations = new List<SubscriptionAPIVersionOperation>();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SubscriptionAPIVersionOperation> Operations { get; set; }
    }
}
