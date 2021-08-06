using Luna.PubSub.Clients;
using Luna.PubSub.Public.Client;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.PubSub.Test
{
    public class MockStorageUtils : IAzureStorageUtils
    {
        public const string CONNECTION_STRING_FORMAT = "connection_string_table_{0}";

        public Dictionary<string, List<LunaBaseEventEntity>> TableClient { get; set; }
        public Dictionary<string, List<string>> QueueClient { get; set; }

        public MockStorageUtils()
        {
            TableClient = new Dictionary<string, List<LunaBaseEventEntity>>();
            QueueClient = new Dictionary<string, List<string>>();
        }

        public async Task CreateQueueMessage(string queueName, string messageText)
        {
            if (!this.QueueClient.ContainsKey(queueName))
            {
                this.QueueClient.Add(queueName, new List<string>());
            }

            this.QueueClient[queueName].Add(messageText);
        }

        public async Task<string> GetReadOnlyTableSaSConnectionString(string tableName, int validInHours = 1)
        {
            return string.Format(CONNECTION_STRING_FORMAT, tableName);
        }

        public async Task<TableResult> InsertTableEntity(string tableName, TableEntity entity)
        {
            if (!this.TableClient.ContainsKey(tableName))
            {
                this.TableClient.Add(tableName, new List<LunaBaseEventEntity>());
            }

            if (!(entity is LunaBaseEventEntity))
            {
                throw new NotSupportedException("Only support LunaBaseEventEntity.");
            }

            this.TableClient[tableName].Add((LunaBaseEventEntity)entity);

            return new TableResult();
        }

        public async Task<List<LunaBaseEventEntity>> RetrieveSortedTableEntities(string tableName, 
            string eventType, 
            long eventsAfter = 0, 
            string partitionKey = null)
        {
            if (!this.TableClient.ContainsKey(tableName))
            {
                return new List<LunaBaseEventEntity>();
            }

            var eventList = this.TableClient[tableName];

            if (partitionKey == null)
            {
                return eventList.
                    Where(x => x.EventType == eventType && 
                        x.EventSequenceId > eventsAfter).ToList();
            }
            else
            {
                return eventList.
                    Where(x => x.EventType == eventType && 
                        x.EventSequenceId > eventsAfter && 
                        x.PartitionKey == partitionKey).ToList();
            }
        }
    }
}
