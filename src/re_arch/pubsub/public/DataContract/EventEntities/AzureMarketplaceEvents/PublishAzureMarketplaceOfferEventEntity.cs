using Luna.PubSub.PublicClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client.DataContract
{
    public class PublishAzureMarketplaceOfferEventEntity : AzureMarketplaceOfferEventEntity
    {
        public PublishAzureMarketplaceOfferEventEntity()
        {
            this.EventType = LunaEventType.PUBLISH_AZURE_MARKETPLACE_OFFER;
        }

        public PublishAzureMarketplaceOfferEventEntity(string offerId, string content) : 
            base(offerId, content)
        {
            this.EventType = LunaEventType.PUBLISH_AZURE_MARKETPLACE_OFFER;
        }
    }
}
