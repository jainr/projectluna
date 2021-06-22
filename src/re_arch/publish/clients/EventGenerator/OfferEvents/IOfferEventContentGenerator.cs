using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Publish.Clients
{
    /// <summary>
    /// The client interface to generate offer events
    /// </summary>
    public interface IOfferEventContentGenerator
    {
        /// <summary>
        /// Generate create marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="name">The offer name</param>
        /// <param name="properties">The offer properties</param>
        /// <returns>The event content JSON string</returns>
        string GenerateCreateMarketplaceOfferEventContent(
            string name,
            MarketplaceOfferProp properties);

        /// <summary>
        /// Generate create marketplace offer from template event and convert to JSON string
        /// </summary>
        /// <param name="template">The offer template</param>
        /// <returns>The event content JSON string</returns>
        string GenerateCreateMarketplaceOfferFromTemplateEventContent(
            string template);

        /// <summary>
        /// Generate update marketplace offer from template event and convert to JSON string
        /// </summary>
        /// <param name="template">The offer template</param>
        /// <returns>The event content JSON string</returns>
        string GenerateUpdateMarketplaceOfferFromTemplateEventContent(
            string template);

    }
}
