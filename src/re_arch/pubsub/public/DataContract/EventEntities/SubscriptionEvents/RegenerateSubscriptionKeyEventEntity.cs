using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public class RegenerateSubscriptionKeyEventEntity : SubscriptionEventEntity
    {
        public RegenerateSubscriptionKeyEventEntity()
        {
            EventType = LunaEventType.REGENERATE_SUBSCRIPTION_KEY_EVENT;
        }

        public RegenerateSubscriptionKeyEventEntity(string subscriptionId, string content) : base(subscriptionId, content)
        {
            EventType = LunaEventType.REGENERATE_SUBSCRIPTION_KEY_EVENT;
        }
    }
}
