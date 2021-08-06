using Luna.PubSub.Public.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.PubSub.Clients
{
    public class PubSubFunctionsImpl : IPubSubFunctionsImpl
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ILogger<PubSubFunctionsImpl> _logger;

        public PubSubFunctionsImpl(
            IEventStoreClient eventStoreClient,
            ILogger<PubSubFunctionsImpl> logger)
        {
            this._eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EventStoreInfo> GetEventStoreInfoAsync(string name)
        {
            var eventStore = await _eventStoreClient.GetEventStoreConnectionInfo(name);
            return eventStore;
        }

        public async Task<List<LunaBaseEventEntity>> ListSortedEventsAsync(string name, 
            string eventType, 
            long eventsAfter, 
            string partitionKey)
        {
            var events = await _eventStoreClient.ListEvents(name, eventType, eventsAfter, partitionKey);
            return events;
        }

        public async Task<LunaBaseEventEntity> PublishEventAsync(string name, string content)
        {
            var ev = await _eventStoreClient.PublishEvent(name, content);
            return ev;
        }
    }
}
