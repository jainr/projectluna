﻿using Luna.Marketplace.Data;
using Luna.Marketplace.Public.Client;
using System.Collections.Generic;

namespace Luna.Marketplace.Clients
{
    public interface IOfferEventProcessor
    {
        /// <summary>
        /// Get marketplace offer from a snapshot and events
        /// </summary>
        /// <param name="offerId">The id of the offer</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        MarketplaceOffer GetMarketplaceOffer(
            string offerId, 
            List<BaseMarketplaceEvent> events,
            MarketplaceOfferSnapshotDB snapshot = null);

        /// <summary>
        /// Get marketplace offer in JSON string from a snapshot and events
        /// </summary>
        /// <param name="offerId">The id of the offer</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        string GetMarketplaceOfferJSONString(
            string offerId,
            List<BaseMarketplaceEvent> events,
            MarketplaceOfferSnapshotDB snapshot = null);
    }
}
