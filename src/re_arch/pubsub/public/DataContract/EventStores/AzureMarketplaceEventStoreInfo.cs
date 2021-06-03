using Luna.PubSub.PublicClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public class AzureMarketplaceEventStoreInfo : EventStoreInfo
    {
        public AzureMarketplaceEventStoreInfo(string connectionString, DateTime? validThrough): 
            base(LunaEventStoreType.AZURE_MARKETPLACE_EVENT_STORE, connectionString, validThrough)
        {
            ValidEventTypes.Add(LunaEventType.PUBLISH_AZURE_MARKETPLACE_OFFER);
            ValidEventTypes.Add(LunaEventType.DELETE_AZURE_MARKETPLACE_OFFER);
            ValidEventTypes.Add(LunaEventType.CREATE_AZURE_MARKETPLACE_SUBSCRIPTION);
            ValidEventTypes.Add(LunaEventType.ACTIVATE_AZURE_MARKETPLACE_SUBSCRIPTION);
            ValidEventTypes.Add(LunaEventType.DELETE_AZURE_MARKETPLACE_SUBSCRIPTION);

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "gallery",
                SubscriberFunctionName = "processazuremarketplaceevents",
                ExcludedEventTypes = new List<string>(new string[] 
                { 
                    LunaEventType.CREATE_AZURE_MARKETPLACE_SUBSCRIPTION,  
                    LunaEventType.DELETE_AZURE_MARKETPLACE_SUBSCRIPTION
                })
            });

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "provision",
                SubscriberFunctionName = "processazuremarketplaceevents",
                ExcludedEventTypes = new List<string>(new string[]
                {
                    LunaEventType.ACTIVATE_AZURE_MARKETPLACE_SUBSCRIPTION
                })
            });
        }
    }
}
