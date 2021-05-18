using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public class DeleteSubscriptionEventEntity : SubscriptionEventEntity
    {
        public DeleteSubscriptionEventEntity()
        {
            EventType = LunaEventType.DELETE_SUBSCRIPTION_EVENT;
        }

        public DeleteSubscriptionEventEntity(string subscriptionId, string content) : base(subscriptionId, content)
        {
            EventType = LunaEventType.DELETE_SUBSCRIPTION_EVENT;
        }
    }
}
