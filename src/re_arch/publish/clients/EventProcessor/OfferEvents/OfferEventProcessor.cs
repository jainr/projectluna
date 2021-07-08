using Luna.Common.Utils;
using Luna.Publish.Data;
using Luna.Publish.Public.Client;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Luna.Publish.Clients
{
    public class OfferEventProcessor : IOfferEventProcessor
    {
        /// <summary>
        /// Get marketplace offer from a snapshot and events
        /// </summary>
        /// <param name="offerId">The id of the offer</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        public MarketplaceOffer GetMarketplaceOffer(
            string offerId, 
            List<BaseMarketplaceOfferEvent> events,
            MarketplaceOfferSnapshotDB snapshot = null)
        {
            MarketplaceOffer result = null;

            if (snapshot != null)
            {
                result = (MarketplaceOffer)JsonConvert.DeserializeObject(snapshot.SnapshotContent, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            else if (events[0].EventType != MarketplaceOfferEventType.CreateMarketplaceOfferFromTemplate)
            {
                throw new LunaServerException($"The snapshot of marketplace offer {offerId} is null.");
            }

            foreach (var ev in events)
            {
                switch (ev.EventType)
                {
                    case MarketplaceOfferEventType.CreateMarketplaceOfferFromTemplate:
                        result = ((CreateMarketplaceOfferFromTemplateEvent)ev).Offer;
                        result.Status = MarketplaceOfferStatus.Draft.ToString();
                        break;
                    case MarketplaceOfferEventType.UpdateMarketplaceOfferFromTemplate:
                        result = ((UpdateMarketplaceOfferFromTemplateEvent)ev).Offer;
                        break;
                    case MarketplaceOfferEventType.PublishMarketplaceOffer:
                        result.Status = MarketplaceOfferStatus.Published.ToString();
                        break;
                    case MarketplaceOfferEventType.DeleteMarketplaceOffer:
                        result = null;
                        break;
                    default:
                        throw new LunaServerException($"Unknown event type {ev.EventType.ToString()}.");
                }
            }

            return result;
        }

        /// <summary>
        /// Get marketplace offer in JSON string from a snapshot and events
        /// </summary>
        /// <param name="offerId">The id of the offer</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        public string GetMarketplaceOfferJSONString(
            string offerId,
            List<BaseMarketplaceOfferEvent> events,
            MarketplaceOfferSnapshotDB snapshot = null)
        {
            var offer = GetMarketplaceOffer(offerId, events, snapshot);

            return JsonConvert.SerializeObject(offer, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}
