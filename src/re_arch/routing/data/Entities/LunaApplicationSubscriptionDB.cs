using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Routing.Data
{
    public class LunaApplicationSubscriptionDB
    {
        public long Id { get; set; }

        public string SubscriptionId { get; set; }

        public string PrimaryKeySecretName { get; set; }

        public string SecondaryKeySecretName { get; set; }

        public string ApplicationName { get; set; }

        public string Status { get; set; }

        public long LastAppliedEventId { get; set; }
    }
}
