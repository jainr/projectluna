using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client.DataContracts
{
    public class LunaApplicationSubscription
    {
        public LunaApplicationSubscription()
        {
            this.Owners = new List<LunaApplicationSubscriptionOwner>();
        }

        public Guid SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public DateTime CreatedTime { get; set; }

        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }

        public string BaseUrl { get; set; }

        public string Notes { get; set; }

        public List<LunaApplicationSubscriptionOwner> Owners { get; set; }
    }
}
