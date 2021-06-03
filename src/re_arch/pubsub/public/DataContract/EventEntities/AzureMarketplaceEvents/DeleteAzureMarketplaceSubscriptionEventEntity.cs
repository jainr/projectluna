using Luna.PubSub.PublicClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client.DataContract
{
    public class DeleteAzureMarketplaceSubscriptionEventEntity : AzureMarketplaceSubscriptionEventEntity
    {
        public DeleteAzureMarketplaceSubscriptionEventEntity()
        {
            this.EventType = LunaEventType.DELETE_AZURE_MARKETPLACE_SUBSCRIPTION;
        }

        public DeleteAzureMarketplaceSubscriptionEventEntity(Guid SubscriptionId, string content) : 
            base(SubscriptionId, content)
        {
            this.EventType = LunaEventType.DELETE_AZURE_MARKETPLACE_SUBSCRIPTION;
        }

    }
}
