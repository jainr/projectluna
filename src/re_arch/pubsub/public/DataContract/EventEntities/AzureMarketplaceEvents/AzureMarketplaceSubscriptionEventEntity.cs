using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public abstract class AzureMarketplaceSubscriptionEventEntity : LunaBaseEventEntity
    {
        public AzureMarketplaceSubscriptionEventEntity()
        {
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Reset the partition key and row key after deserialization
            PartitionKey = SubscriptionId.ToString();
            RowKey = Guid.NewGuid().ToString();
            EventId = RowKey;

            // Reset the time
            CreatedTime = DateTime.UtcNow;
            EventSequenceId = CreatedTime.Ticks;
        }

        public AzureMarketplaceSubscriptionEventEntity(Guid subscriptionId, string content)
        {
            PartitionKey = subscriptionId.ToString();
            RowKey = Guid.NewGuid().ToString();

            CreatedTime = DateTime.UtcNow;
            EventSequenceId = CreatedTime.Ticks;
            SubscriptionId = subscriptionId;
            EventId = RowKey;
            EventContent = content;
        }

        public string EventId { get; set; }

        public Guid SubscriptionId { get; set; }

    }
}
