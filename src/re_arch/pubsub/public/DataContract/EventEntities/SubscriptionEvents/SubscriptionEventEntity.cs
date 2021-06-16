using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class SubscriptionEventEntity : LunaBaseEventEntity
    {

        public SubscriptionEventEntity()
        {
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Reset the partition key and row key after deserialization
            PartitionKey = SubscriptionId;
            RowKey = Guid.NewGuid().ToString();
            EventId = RowKey;

            // Reset the time
            CreatedTime = DateTime.UtcNow;
            EventSequenceId = CreatedTime.Ticks;
        }

        public SubscriptionEventEntity(string subscriptionId, string content)
        {
            PartitionKey = subscriptionId;
            RowKey = Guid.NewGuid().ToString();

            CreatedTime = DateTime.UtcNow;
            EventSequenceId = CreatedTime.Ticks;
            SubscriptionId = subscriptionId;
            EventId = RowKey;
            EventContent = content;
        }

        public string EventId { get; set; }

        public string SubscriptionId { get; set; }
    }
}
