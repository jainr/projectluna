using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public class CreateSubscriptionEventEntity : SubscriptionEventEntity
    {
        public CreateSubscriptionEventEntity()
        {
            EventType = LunaEventType.CREATE_SUBSCRIPTION_EVENT;
        }

        public CreateSubscriptionEventEntity(string subscriptionId, string content) : base(subscriptionId, content)
        {
            EventType = LunaEventType.CREATE_SUBSCRIPTION_EVENT;
        }
    }
}
