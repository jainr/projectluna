using Luna.PubSub.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.PubSub.Clients
{
    public interface IEventStoreClient
    {

        /// <summary>
        /// Get the connection string for specified event store
        /// </summary>
        /// <param name="name">The event store name</param>
        /// <returns>The event store info</returns>
        Task<EventStoreInfo> GetEventStoreConnectionInfo(string name);

        /// <summary>
        /// Publish an event to the specified event store
        /// </summary>
        /// <param name="eventStoreName">The event store name</param>
        /// <param name="content">The content of the event</param>
        /// <returns>The published event</returns>
        Task<LunaBaseEventEntity> PublishEvent(string eventStoreName, string content);

        /// <summary>
        /// List events from the specified event store
        /// </summary>
        /// <param name="eventStoreName">The event store name</param>
        /// <param name="eventType">The event type</param>
        /// <param name="eventsAfter">Only list events published after a certain event</param>
        /// <param name="partitionKey">Only list events with certain partition key</param>
        /// <returns>The list of events sorted by published time</returns>
        Task<List<LunaBaseEventEntity>> ListEvents(
            string eventStoreName, 
            string eventType = null, 
            long eventsAfter = 0,
            string partitionKey = null);
    }
}
