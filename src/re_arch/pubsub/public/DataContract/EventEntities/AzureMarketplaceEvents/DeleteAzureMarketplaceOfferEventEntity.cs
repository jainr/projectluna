using Luna.PubSub.PublicClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client.DataContract
{
    public class DeleteAzureMarketplaceOfferEventEntity : AzureMarketplaceOfferEventEntity
    {
        public DeleteAzureMarketplaceOfferEventEntity()
        {
            this.EventType = LunaEventType.DELETE_AZURE_MARKETPLACE_OFFER;
        }

        public DeleteAzureMarketplaceOfferEventEntity(string offerId) : base(offerId, string.Empty)
        {
            this.EventType = LunaEventType.DELETE_AZURE_MARKETPLACE_OFFER;
        }
    }
}
