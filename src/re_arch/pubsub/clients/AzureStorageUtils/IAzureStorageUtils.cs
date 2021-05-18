using Luna.PubSub.PublicClient;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.PubSub.Utils
{
    public interface IAzureStorageUtils
    {
        /// <summary>
        /// Insert an entity to a table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="entity">The entity</param>
        /// <returns></returns>
        Task<TableResult> InsertTableEntity(string tableName, TableEntity entity);

        /// <summary>
        /// Retrive a table entity
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="eventType">The event type</param>
        /// <param name="eventsAfter">The earliest event sequence id</param>
        /// <returns></returns>
        Task<List<LunaBaseEventEntity>> RetrieveSortedTableEntities(string tableName, string eventType, long eventsAfter = 0);

        /// <summary>
        /// Get ReadOnly SaS connection string for a table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="validInHours">The valid time in hours</param>
        /// <returns>The SaS connection string</returns>
        Task<string> GetReadOnlyTableSaSConnectionString(string tableName, int validInHours = 1);

        /// <summary>
        /// Create a new message in the specified queue
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="messageText">The message text</param>
        /// <returns></returns>
        Task CreateQueueMessage(string queueName, string messageText);
    }
}
