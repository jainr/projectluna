using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Common.Utils;

namespace Luna.Publish.Public.Client
{
    public interface IPublishServiceClient
    {

        /// <summary>
        /// Regenerate application master keys
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="keyName">The key name</param>
        /// <param name="headers">the luna request header</param>
        /// <returns></returns>
        Task<LunaApplicationMasterKeys> RegenerateApplicationMasterKeys(string appName, string keyName, LunaRequestHeaders headers);

        /// <summary>
        /// List Luna applications owned by the caller
        /// </summary>
        /// <param name="isAdmin">If current user is admin</param>
        /// <param name="headers">The luna request header containing user id</param>
        /// <returns>All Luna applications owned by the caller</returns>
        Task<List<LunaApplication>> ListLunaApplications(bool isAdmin, LunaRequestHeaders headers);

        /// <summary>
        /// Get Luna applciation master keys
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The application master keys</returns>
        Task<LunaApplicationMasterKeys> GetApplicationMasterKeys(
            string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API Version properties</returns>
        Task<LunaApplicationProp> CreateLunaApplication(
            string name,
            string properties,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create Luna API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API properties</returns>
        Task<BaseLunaAPIProp> CreateLunaAPI(
            string appName,
            string apiName,
            string properties,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create Luna API version
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="versionName">The version name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API Version properties</returns>
        Task<BaseAPIVersionProp> CreateLunaAPIVersion(
            string appName,
            string apiName,
            string versionName,
            string properties,
            LunaRequestHeaders headers);


        /// <summary>
        /// Update Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The application properties</returns>
        Task<LunaApplicationProp> UpdateLunaApplication(
            string name,
            string properties,
            LunaRequestHeaders headers);

        /// <summary>
        /// Update Luna API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API properties</returns>
        Task<BaseLunaAPIProp> UpdateLunaAPI(
            string appName,
            string apiName,
            string properties,
            LunaRequestHeaders headers);

        /// <summary>
        /// Update Luna API version
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="versionName">The version name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API Version properties</returns>
        Task<BaseAPIVersionProp> UpdateLunaAPIVersion(
            string appName,
            string apiName,
            string versionName,
            string properties,
            LunaRequestHeaders headers);


        /// <summary>
        /// Delete Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>True if deleted, false otherwise</returns>
        Task<bool> DeleteLunaApplication(
            string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// Delete Luna API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>True if deleted, false otherwise</returns>
        Task<bool> DeleteLunaAPI(
            string appName,
            string apiName,
            LunaRequestHeaders headers);

        /// <summary>
        /// Delete Luna API version
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="versionName">The version name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>True if deleted, false otherwise</returns>
        Task<bool> DeleteLunaAPIVersion(
            string appName,
            string apiName,
            string versionName,
            LunaRequestHeaders headers);


        /// <summary>
        /// Publish Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="comments">The publish comments</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>True if the application is published, false otherwise</returns>
        Task<bool> PublishLunaApplication(
            string name,
            string comments,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get a Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The Luna application properties</returns>
        Task<LunaApplication> GetLunaApplication(
            string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create or update Azure marketplace offer from template
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="template">The offer template</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task<MarketplaceOffer> CreateOrUpdateMarketplaceOfferFromTemplateAsync(string name,
            string template,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="offer">The offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task<AzureMarketplaceOffer> CreateMarketplaceOfferAsync(string name, 
            AzureMarketplaceOffer offer, 
            LunaRequestHeaders headers);

        /// <summary>
        /// Update an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="offer">The offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task<AzureMarketplaceOffer> UpdateMarketplaceOfferAsync(string name,
            AzureMarketplaceOffer offer,
            LunaRequestHeaders headers);

        /// <summary>
        /// Publish an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        Task PublishMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        Task<MarketplaceOffer> GetMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// List Azure marketplace offers
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        Task<List<MarketplaceOffer>> ListMarketplaceOffersAsync(LunaRequestHeaders headers);

        /// <summary>
        /// Delete an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns></returns>
        Task DeleteMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers);

        /// Create a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="plan">The plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        Task<AzureMarketplacePlan> CreateMarketplacePlanAsync(string offerName,
            string planName,
            AzureMarketplacePlan plan,
            LunaRequestHeaders headers);

        /// Update a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="plan">The plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        Task<AzureMarketplacePlan> UpdateMarketplacePlanAsync(string offerName,
            string planName,
            AzureMarketplacePlan plan,
            LunaRequestHeaders headers);

        /// Delete a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        Task DeleteMarketplacePlanAsync(string offerName,
            string planName,
            LunaRequestHeaders headers);

        /// Get a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan</returns>
        Task<AzureMarketplacePlan> GetMarketplacePlanAsync(string offerName,
            string planName,
            LunaRequestHeaders headers);

        /// List plans in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plans</returns>
        Task<List<AzureMarketplacePlan>> ListMarketplacePlansAsync(string offerName,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create an automation webhook
        /// </summary>
        /// <param name="name">Name of the automation webhook</param>
        /// <param name="webhook">The webhook</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The webhook created</returns>
        Task<AutomationWebhook> CreateAutomationWebhookAsync(string name,
            AutomationWebhook webhook,
            LunaRequestHeaders headers);

        /// <summary>
        /// Update an automation webhook
        /// </summary>
        /// <param name="name">Name of the automation webhook</param>
        /// <param name="webhook">The webhook</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The webhook updated</returns>
        Task<AutomationWebhook> UpdateAutomationWebhookAsync(string name,
            AutomationWebhook webhook,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get an automation webhook
        /// </summary>
        /// <param name="name">Name of the automation webhook</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The webhook</returns>
        Task<AutomationWebhook> GetAutomationWebhookAsync(string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// List automation webhooks
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The webhooks</returns>
        Task<List<AutomationWebhook>> ListAutomationWebhooksAsync(LunaRequestHeaders headers);

        /// <summary>
        /// Delete an automation webhook
        /// </summary>
        /// <param name="name">Name of the automation webhook</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns></returns>
        Task DeleteAutomationWebhookAsync(string name,
            LunaRequestHeaders headers);
    }
}
