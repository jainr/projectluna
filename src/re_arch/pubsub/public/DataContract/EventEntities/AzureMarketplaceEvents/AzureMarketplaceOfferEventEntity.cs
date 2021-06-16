using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public abstract class AzureMarketplaceOfferEventEntity : LunaBaseEventEntity
    {
        public AzureMarketplaceOfferEventEntity()
        {
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Reset the partition key and row key after deserialization
            PartitionKey = MarketplaceOfferId;
            RowKey = Guid.NewGuid().ToString();
            EventId = RowKey;

            // Reset the time
            CreatedTime = DateTime.UtcNow;
            EventSequenceId = CreatedTime.Ticks;
        }

        public AzureMarketplaceOfferEventEntity(string offerId, string content)
        {
            PartitionKey = offerId;
            RowKey = Guid.NewGuid().ToString();

            CreatedTime = DateTime.UtcNow;
            EventSequenceId = CreatedTime.Ticks;
            MarketplaceOfferId = offerId;
            EventId = RowKey;
            EventContent = content;
        }

        public string EventId { get; set; }

        public string MarketplaceOfferId { get; set; }

    }
}
