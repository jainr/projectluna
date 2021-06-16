using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class CreateAzureMarketplaceSubscriptionEventEntity : AzureMarketplaceSubscriptionEventEntity
    {
        public CreateAzureMarketplaceSubscriptionEventEntity()
        {
            this.EventType = LunaEventType.CREATE_AZURE_MARKETPLACE_SUBSCRIPTION;
        }

        public CreateAzureMarketplaceSubscriptionEventEntity(Guid SubscriptionId, string content) : 
            base(SubscriptionId, content)
        {
            this.EventType = LunaEventType.CREATE_AZURE_MARKETPLACE_SUBSCRIPTION;
        }
    }
}
