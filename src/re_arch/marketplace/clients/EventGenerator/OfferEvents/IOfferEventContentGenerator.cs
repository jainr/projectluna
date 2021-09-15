using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Marketplace.Clients
{
    /// <summary>
    /// The client interface to generate offer events
    /// </summary>
    public interface IOfferEventContentGenerator
    {
        /// <summary>
        /// Generate create marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer name</param>
        /// <param name="offerReq">The offer properties</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateCreateMarketplaceOfferEventContentAsync(
            string offerId,
            MarketplaceOfferProp offerReq);

        /// <summary>
        /// Generate update marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer name</param>
        /// <param name="offerReq">The offer properties</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateUpdateMarketplaceOfferEventContentAsync(
            string offerId,
            MarketplaceOfferProp offerReq);

        /// <summary>
        /// Generate delete marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer name</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateDeleteMarketplaceOfferEventContentAsync(
            string offerId);

        /// <summary>
        /// Generate publish marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer name</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GeneratePublishMarketplaceOfferEventContentAsync(
            string offerId);

        /// <summary>
        /// Generate create marketplace plan event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan Id</param>
        /// <param name="planReq">The plan properties</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateCreateMarketplacePlanEventContentAsync(
            string offerId,
            string planId,
            MarketplacePlanProp planReq);

        /// <summary>
        /// Generate update marketplace plan event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan Id</param>
        /// <param name="planReq">The plan properties</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateUpdateMarketplacePlanEventContentAsync(
            string offerId,
            string planId,
            MarketplacePlanProp planReq);

        /// <summary>
        /// Generate delete marketplace plan event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan Id</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateDeleteMarketplacePlanEventContentAsync(
            string offerId,
            string planId);

        /// <summary>
        /// Generate create offer parameter event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="paramReq">The parameter properties</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateCreateOfferParameterEventContentAsync(
            string offerId,
            string parameterName,
            MarketplaceParameter paramReq);

        /// <summary>
        /// Generate update offer parameter event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="paramReq">The parameter properties</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateUpdateOfferParameterEventContentAsync(
            string offerId,
            string parameterName,
            MarketplaceParameter paramReq);

        /// <summary>
        /// Generate delete offer parameter event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateDeleteOfferParameterEventContentAsync(
            string offerId,
            string parameterName);

        /// <summary>
        /// Generate create provisiong step event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="stepName">The provision step name</param>
        /// <param name="stepType">The type of the provisioning step</param>
        /// <param name="stepReq">The provision step properties</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateCreateProvisoningStepEventContentAsync(
            string offerId,
            string stepName,
            string stepType,
            BaseProvisioningStepProp stepReq);

        /// <summary>
        /// Generate update provisiong step event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="stepName">The provision step name</param>
        /// <param name="stepType">The type of the provisioning step</param>
        /// <param name="stepReq">The provision step properties</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateUpdateProvisoningStepEventContentAsync(
            string offerId,
            string stepName,
            string stepType,
            BaseProvisioningStepProp stepReq);

        /// <summary>
        /// Generate delete provisiong step event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="stepName">The provision step name</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateDeleteProvisoningStepEventContentAsync(
            string offerId,
            string stepName);

        /// <summary>
        /// Generate create marketplace offer from template event and convert to JSON string
        /// </summary>
        /// <param name="template">The offer template</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateCreateMarketplaceOfferFromTemplateEventContentAsync(
            string template);

        /// <summary>
        /// Generate update marketplace offer from template event and convert to JSON string
        /// </summary>
        /// <param name="template">The offer template</param>
        /// <returns>The event content JSON string</returns>
        Task<string> GenerateUpdateMarketplaceOfferFromTemplateEventContentAsync(
            string template);

    }
}
