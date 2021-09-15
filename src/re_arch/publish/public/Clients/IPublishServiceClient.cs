using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Common.Utils;
using Newtonsoft.Json.Linq;

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
        Task<LunaApplicationMasterKeysResponse> RegenerateApplicationMasterKeys(string appName, string keyName, LunaRequestHeaders headers);

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
        Task<LunaApplicationMasterKeysResponse> GetApplicationMasterKeys(
            string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// Create Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The Luna application response</returns>
        Task<string> CreateLunaApplication(
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
        Task<string> CreateLunaAPI(
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
        Task<string> UpdateLunaApplication(
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
        Task<string> GetLunaApplication(
            string name,
            LunaRequestHeaders headers);

        /// Get a Luna API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The api name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The Luna api properties</returns>
        Task<string> GetLunaAPI(
            string appName,
            string apiName,
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
