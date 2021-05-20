using Luna.PubSub.PublicClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public class ApplicationEventStoreInfo : EventStoreInfo
    {
        public ApplicationEventStoreInfo(string connectionString, DateTime? validThrough): 
            base(LunaEventStoreType.APPLICATION_EVENT_STORE, connectionString, validThrough)
        {
            ValidEventTypes.Add(LunaEventType.PUBLISH_APPLICATION_EVENT);
            ValidEventTypes.Add(LunaEventType.REGENERATE_APPLICATION_MASTER_KEY);
            ValidEventTypes.Add(LunaEventType.DELETE_APPLICATION_EVENT);

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "routing",
                SubscriberFunctionName = "processapplicationevents"
            });

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "gallery",
                SubscriberFunctionName = "processapplicationevents"
            });

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "provision",
                SubscriberFunctionName = "processapplicationevents"
            });
        }
    }
}
