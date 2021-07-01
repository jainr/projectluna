using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class LunaApplicationSubscriptionEventContent
    {
        public LunaApplicationSubscriptionEventContent()
        {

        }

        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public string ApplicationName { get; set; }

        public string Status { get; set; }

        public string Notes { get; set; }

        public string PrimaryKeySecretName { get; set; }

        public string SecondaryKeySecretName { get; set; }
    }
}
