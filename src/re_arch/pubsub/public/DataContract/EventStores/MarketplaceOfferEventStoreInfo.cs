using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class MarketplaceOfferEventStoreInfo : EventStoreInfo
    {
        public MarketplaceOfferEventStoreInfo(string connectionString, DateTime? validThrough): 
            base(LunaEventStoreType.AZURE_MARKETPLACE_OFFER_EVENT_STORE, connectionString, validThrough)
        {
            ValidEventTypes.Add(LunaEventType.PUBLISH_AZURE_MARKETPLACE_OFFER);
            ValidEventTypes.Add(LunaEventType.DELETE_AZURE_MARKETPLACE_OFFER);

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "gallery",
                SubscriberFunctionName = "processmarketplaceofferevents",
                ExcludedEventTypes = new List<string>()
            });

            this.EventSubscribers.Add(new LunaEventSubscriber()
            {
                SubscriberServiceName = "provision",
                SubscriberFunctionName = "processmarketplaceofferevents",
                ExcludedEventTypes = new List<string>()
            });
        }
    }
}
