using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts.Luna.AI
{
    public class SubscriptionAPIVersionOperation
    {
        public SubscriptionAPIVersionOperation()
        {
            Parameters = new List<SubscriptionAPIVersionOperationParameter>();
        }
        public string Name { get; set; }

        public string Description { get; set; }

        public List<SubscriptionAPIVersionOperationParameter> Parameters { get; set; }
    }
}
