using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public abstract class ApplicationEventEntity : LunaBaseEventEntity
    {
        public ApplicationEventEntity()
        {
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Reset the partition key and row key after deserialization
            PartitionKey = ApplicationName;
            RowKey = Guid.NewGuid().ToString();
            EventId = RowKey;

            // Reset the time
            CreatedTime = DateTime.UtcNow;
            EventSequenceId = CreatedTime.Ticks;
        }

        public ApplicationEventEntity(string appName, string content)
        {
            PartitionKey = appName;
            RowKey = Guid.NewGuid().ToString();

            CreatedTime = DateTime.UtcNow;
            EventSequenceId = CreatedTime.Ticks;
            ApplicationName = appName;
            EventId = RowKey;
            EventContent = content;
        }

        public string EventId { get; set; }

        public string ApplicationName { get; set; }

    }
}
