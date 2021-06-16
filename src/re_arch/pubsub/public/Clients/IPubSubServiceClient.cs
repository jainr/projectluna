using Luna.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.PubSub.Public.Client
{
    public interface IPubSubServiceClient
    {
        /// <summary>
        /// Get event store connection string
        /// </summary>
        /// <param name="name">The event store name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The event store connection string and other info</returns>
        Task<EventStoreInfo> GetEventStoreConnectionStringAsync(string name, LunaRequestHeaders headers);

        /// <summary>
        /// List events in the specified event store
        /// </summary>
        /// <param name="eventStoreName">The event store name</param>
        /// <param name="headers">The luna request header</param>
        /// <param name="eventType"></param>
        /// <param name="eventsAfter"></param>
        /// <returns>The event list</returns>
        Task<List<LunaBaseEventEntity>> ListEventsAsync(string eventStoreName, LunaRequestHeaders headers, string eventType = null, long eventsAfter = 0);

        /// <summary>
        /// Publish a event to the specified event store
        /// </summary>
        /// <param name="eventStoreName">The event store name</param>
        /// <param name="ev">The event content</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The published event</returns>
        Task<LunaBaseEventEntity> PublishEventAsync(string eventStoreName, LunaBaseEventEntity ev, LunaRequestHeaders headers);
    }
}
