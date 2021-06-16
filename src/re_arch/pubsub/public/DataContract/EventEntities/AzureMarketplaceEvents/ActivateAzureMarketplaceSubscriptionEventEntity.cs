using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class ActivateAzureMarketplaceSubscriptionEventEntity : AzureMarketplaceSubscriptionEventEntity
    {
        public ActivateAzureMarketplaceSubscriptionEventEntity()
        {
            this.EventType = LunaEventType.ACTIVATE_AZURE_MARKETPLACE_SUBSCRIPTION;
        }

        public ActivateAzureMarketplaceSubscriptionEventEntity(Guid SubscriptionId, string content) : 
            base(SubscriptionId, content)
        {
            this.EventType = LunaEventType.ACTIVATE_AZURE_MARKETPLACE_SUBSCRIPTION;
        }

    }
}
