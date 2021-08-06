using Luna.PubSub.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.PubSub.Clients
{
    public interface IPubSubFunctionsImpl
    {
        Task<EventStoreInfo> GetEventStoreInfoAsync(string name);

        Task<List<LunaBaseEventEntity>> ListSortedEventsAsync(string name, string eventType, long eventsAfter, string partitionKey);

        Task<LunaBaseEventEntity> PublishEventAsync(string name, string content);
    }
}
