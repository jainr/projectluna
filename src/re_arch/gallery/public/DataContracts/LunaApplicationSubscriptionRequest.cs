using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class LunaApplicationSubscriptionRequest
    {
        public string LunaApplicationName { get; set; }

        public string SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public string OwnerId { get; set; }
    }
}
