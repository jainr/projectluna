using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class MarketplaceSubscriptionEventStoreInfo : EventStoreInfo
    {
        public MarketplaceSubscriptionEventStoreInfo(string connectionString, DateTime? validThrough): 
            base(LunaEventStoreType.AZURE_MARKETPLACE_SUB_EVENT_STORE, connectionString, validThrough)
        {
            ValidEventTypes.Add(LunaEventType.CREATE_AZURE_MARKETPLACE_SUBSCRIPTION);
            ValidEventTypes.Add(LunaEventType.ACTIVATE_AZURE_MARKETPLACE_SUBSCRIPTION);
            ValidEventTypes.Add(LunaEventType.DELETE_AZURE_MARKETPLACE_SUBSCRIPTION);

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "provision",
                SubscriberFunctionName = "processmarketplacesubevents",
                ExcludedEventTypes = new List<string>(new string[]
                {
                    LunaEventType.ACTIVATE_AZURE_MARKETPLACE_SUBSCRIPTION
                })
            });
        }
    }
}
