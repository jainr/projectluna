using Luna.Common.Utils.RestClients;
using Luna.Gallery.Public.Client.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Gallery.Public.Client.Clients
{
    public interface IGalleryServiceClient
    {

        /// <summary>
        /// List all luna applications
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The list of Luna applications</returns>
        Task<List<PublishedLunaApplication>> ListLunaApplications(LunaRequestHeaders headers);

        /// <summary>
        /// Get a Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application</returns>
        Task<PublishedLunaApplication> GetLunaApplication(string appName, LunaRequestHeaders headers);


        /// <summary>
        /// Get swagger from a Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application swagger</returns>
        Task<LunaApplicationSwagger> GetLunaApplicationSwagger(string appName, LunaRequestHeaders headers);

        /// <summary>
        /// Get recommended Luna application based on current application
        /// </summary>
        /// <param name="appName">The current application name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The recommended Luna applications</returns>
        Task<List<PublishedLunaApplication>> GetRecommendedLunaApplications(
            string appName, 
            LunaRequestHeaders headers);

        /// <summary>
        /// Create a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionName">The subscription name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription</returns>
        Task<LunaApplicationSubscription> CreateLunaApplicationSubscription(
            string appName, 
            string subscriptionName, 
            LunaRequestHeaders headers);

        /// <summary>
        /// List subscriptions for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscriptions</returns>
        Task<List<LunaApplicationSubscription>> ListLunaApplicationSubscription(
            string appName,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription</returns>
        Task<LunaApplicationSubscription> GetLunaApplicationSubscription(
            string appName, 
            string subscriptionNameOrId, 
            LunaRequestHeaders headers);

        /// <summary>
        /// Delete a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns></returns>
        Task DeleteLunaApplicationSubscription(
            string appName, 
            string subscriptionNameOrId, 
            LunaRequestHeaders headers);

        /// <summary>
        /// Update notes fpr a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="notes">The subscription notes</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription notes</returns>
        Task<LunaApplicationSubscriptionNotes> UpdateLunaApplicationSubscriptionNotes(
            string appName, 
            string subscriptionNameOrId, 
            string notes, 
            LunaRequestHeaders headers);

        /// <summary>
        /// Add a owner a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="userId">The user id for the new owner</param>
        /// <param name="userName">The user name for the new owner</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription owner</returns>
        Task<LunaApplicationSubscriptionOwner> AddLunaApplicationSubscriptionOwner(
            string appName, 
            string subscriptionNameOrId, 
            string userId,
            string userName,
            LunaRequestHeaders headers);

        /// <summary>
        /// Remove a owner a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="userId">The user id for the new owner</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription owner</returns>
        Task<LunaApplicationSubscriptionOwner> RemoveLunaApplicationSubscriptionOwner(
            string appName,
            string subscriptionNameOrId,
            string userId,
            LunaRequestHeaders headers);

        /// <summary>
        /// Regenerate the API key for a subscription for Luna application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="subscriptionNameOrId">The subscription name or id</param>
        /// <param name="keyName">The name of the key</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The Luna application subscription keys</returns>
        Task<LunaApplicationSubscriptionKeys> RegenerateLunaApplicationSubscriptionKey(
            string appName,
            string subscriptionNameOrId,
            string keyName,
            LunaRequestHeaders headers);
    }
}
