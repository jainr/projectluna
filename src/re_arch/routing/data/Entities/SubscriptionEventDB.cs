using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Routing.Data
{
    public class SubscriptionEventDB
    {
        public long Id { get; set; }

        public string SubscriptionId { get; set; }

        public long LastAppliedEventId { get; set; }
    }
}
