using Luna.Common.Utils;
using Luna.PubSub.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Test
{
    public class MockPubSubServiceClient : IPubSubServiceClient
    {
        private readonly Dictionary<string, List<LunaBaseEventEntity>> _eventStores;

        public MockPubSubServiceClient()
        {
            this._eventStores = new Dictionary<string, List<LunaBaseEventEntity>>();
        }

        public async Task<EventStoreInfo> GetEventStoreConnectionStringAsync(string name, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        public async Task<List<LunaBaseEventEntity>> ListEventsAsync(string eventStoreName, LunaRequestHeaders headers, string eventType = null, long eventsAfter = 0, string partitionKey = null)
        {
            if (!this._eventStores.ContainsKey(eventStoreName))
            {
                return new List<LunaBaseEventEntity>();
            }

            return this._eventStores[eventStoreName].Where(x => (eventType == null || eventType == x.EventType) &&
                (x.EventSequenceId > eventsAfter) &&
                (partitionKey == null || x.PartitionKey == partitionKey)).ToList();
        }

        public async Task<LunaBaseEventEntity> PublishEventAsync(string eventStoreName, LunaBaseEventEntity ev, LunaRequestHeaders headers)
        {
            if (!this._eventStores.ContainsKey(eventStoreName))
            {
                this._eventStores.Add(eventStoreName, new List<LunaBaseEventEntity>());
            }

            this._eventStores[eventStoreName].Add(ev);

            return ev;
        }
    }
}
