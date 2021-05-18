using Luna.Common.LoggingUtils;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.PubSub.PublicClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.PubSub.Utils
{
    public class EventStoreClient : IEventStoreClient
    {
        private readonly IAzureStorageUtils _storageUtils;
        private readonly ILogger<EventStoreClient> _logger;
        public EventStoreClient(IAzureStorageUtils storageUtils, ILogger<EventStoreClient> logger)
        {
            this._storageUtils = storageUtils ?? throw new ArgumentNullException(nameof(storageUtils));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get the connection string for specified event store
        /// </summary>
        /// <param name="name">The event store name</param>
        /// <returns>The event store info</returns>
        public async Task<EventStoreInfo> GetEventStoreConnectionInfo(string name)
        {
            if (!LunaEventStoreType.IsValidEventStoreType(name))
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.EVENT_STORE_DOES_NOT_EXIST, name),
                    UserErrorCode.InvalidParameter);
            }

            var validThrough = DateTime.UtcNow.AddHours(1);
            var connectionString = await _storageUtils.GetReadOnlyTableSaSConnectionString(name);
            return GetEventStoreInfoByName(name, connectionString, validThrough);
        }

        /// <summary>
        /// Publish an event to the specified event store
        /// </summary>
        /// <param name="eventStoreName">The event store name</param>
        /// <param name="content">The content of the event</param>
        /// <returns>The published event</returns>
        public async Task<LunaBaseEventEntity> PublishEvent(string eventStoreName, string content)
        {
            var eventStore = GetEventStoreInfoByName(eventStoreName);

            var ev = JsonConvert.DeserializeObject<LunaBaseEventEntity>(content, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (ev == null)
            {
                throw new LunaServerException("Invalid event.");
            }

            if (!eventStore.IsValidEventType(ev.EventType))
            {
                throw new LunaServerException(
                    string.Format(ErrorMessages.EVENT_TYPE_IS_NOT_SUPPORTED, ev.EventType, eventStoreName));
            }

            await _storageUtils.InsertTableEntity(eventStoreName, ev);

            foreach (string queueName in eventStore.GetSubscriberQueueNames())
            {
                await _storageUtils.CreateQueueMessage(queueName, ev.EventType);
            }

            return ev;
        }

        /// <summary>
        /// List events from the specified event store
        /// </summary>
        /// <param name="eventStoreName">The event store name</param>
        /// <param name="eventType">The event type</param>
        /// <param name="eventsAfter">Only list events published after a certain event</param>
        /// <returns>The list of events sorted by published time</returns>
        public async Task<List<LunaBaseEventEntity>> ListEvents(string eventStoreName, string eventType = null, long eventsAfter = 0)
        {
            var events = await _storageUtils.RetrieveSortedTableEntities(eventStoreName, eventType, eventsAfter);
            return events;
        }

        private EventStoreInfo GetEventStoreInfoByName(string name, string connectionString = null, DateTime? validThrough = null)
        {
            if (name.Equals(LunaEventStoreType.APPLICATION_EVENT_STORE, StringComparison.InvariantCultureIgnoreCase))
            {
                return new ApplicationEventStoreInfo(connectionString, validThrough);
            }
            else if (name.Equals(LunaEventStoreType.SUBSCRIPTION_EVENT_STORE, StringComparison.InvariantCultureIgnoreCase))
            {
                return new SubscriptionEventStoreInfo(connectionString, validThrough);
            }
            else
            {
                throw new NotImplementedException($"Event store type {name} is not implemented.");
            }
        }
    }
}
