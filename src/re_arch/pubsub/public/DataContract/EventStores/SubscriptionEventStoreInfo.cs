using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class SubscriptionEventStoreInfo : EventStoreInfo
    {
        public SubscriptionEventStoreInfo(string connectionString, DateTime? validThrough) :
            base(LunaEventStoreType.SUBSCRIPTION_EVENT_STORE, connectionString, validThrough)
        {
            ValidEventTypes.Add(LunaEventType.CREATE_SUBSCRIPTION_EVENT);
            ValidEventTypes.Add(LunaEventType.DELETE_SUBSCRIPTION_EVENT);
            ValidEventTypes.Add(LunaEventType.REGENERATE_SUBSCRIPTION_KEY_EVENT);

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "routing",
                SubscriberFunctionName = "processsubscriptionevents"
            });

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "provision",
                SubscriberFunctionName = "processsubscriptionevents"
            });
        }
    }
}
