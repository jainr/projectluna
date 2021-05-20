using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.LoggingUtils;
using Luna.Common.Utils.HttpUtils;
using Luna.Common.Utils.RestClients;
using System.Collections.Generic;
using Luna.Common.Utils.Azure.AzureKeyvaultUtils;
using Luna.Common.Utils.LoggingUtils;
using Luna.PubSub.PublicClient.Clients;
using Luna.PubSub.PublicClient;
using Luna.Gallery.Data.Entities;
using Luna.Publish.Public.Client.DataContract;
using Luna.Gallery.Public.Client.DataContracts;

namespace Luna.Gallery.Functions
{
    /// <summary>
    /// The service maintains all routings
    /// </summary>
    public class GalleryServiceFunctions
    {
        private const string ROUTING_SERVICE_BASE_URL_CONFIG_NAME = "ROUTING_SERVICE_BASE_URL";

        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<GalleryServiceFunctions> _logger;
        private readonly IPubSubServiceClient _pubSubClient;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;

        public GalleryServiceFunctions(ISqlDbContext dbContext, 
            ILogger<GalleryServiceFunctions> logger, 
            IAzureKeyVaultUtils keyVaultUtils,
            IPubSubServiceClient pubSubClient)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(dbContext));
            this._pubSubClient = pubSubClient ?? throw new ArgumentNullException(nameof(pubSubClient));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
        }

        [FunctionName("ProcessApplicationEvents")]
        public async Task ProcessApplicationEvents([QueueTrigger("gallery-processapplicationevents")] string myQueueItem)
        {
            // Get the last applied event id
            // If there's no record in the database, it will return the default value of long type 0
            var lastAppliedEventId = await _dbContext.PublishedLunaAppliations.
                OrderByDescending(x => x.LastAppliedEventId).
                Select(x => x.LastAppliedEventId).FirstOrDefaultAsync();

            var events = await _pubSubClient.ListEventsAsync(
                LunaEventStoreType.APPLICATION_EVENT_STORE,
                new LunaRequestHeaders(),
                eventsAfter: lastAppliedEventId);

            foreach (var ev in events)
            {
                if (ev.EventType.Equals(LunaEventType.PUBLISH_APPLICATION_EVENT))
                {
                    await CreateOrUpdateLunaApplication(ev.EventContent, ev.EventSequenceId);
                }
                else if (ev.EventType.Equals(LunaEventType.DELETE_APPLICATION_EVENT))
                {
                    // TODO: should not use partition key
                    await DeleteLunaApplication(ev.PartitionKey, ev.EventSequenceId);
                }
            }
        }

        /// <summary>
        /// Test endpoint
        /// </summary>
        /// <param name="req">The http request</param>
        /// <returns></returns>
        [FunctionName("Test")]
        public async Task<IActionResult> Test(
        [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "test")]
        HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.Test));

                try
                {
                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.Test));
                }
            }
        }

        #region published application operations
        /// <summary>
        /// List published applications
        /// </summary>
        /// <group>Application</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applications</url>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="PublishedLunaApplication"/>
        ///     <example>
        ///         <value>
        ///             <see cref="PublishedLunaApplication.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of published Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListPublishedApplications")]
        public async Task<IActionResult> ListPublishedApplications(
        [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "applications")]
        HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListPublishedApplications));

                try
                {
                    var applications = await _dbContext.PublishedLunaAppliations.
                        Where(x => x.IsEnabled).
                        Select(x => x.ToPublishedLunaApplication()).
                        ToListAsync();

                    return new OkObjectResult(applications);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListPublishedApplications));
                }
            }
        }

        /// <summary>
        /// Get recommended Luna applications
        /// </summary>
        /// <group>Application</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applications/{name}/recommended</url>
        /// <param name="name" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="PublishedLunaApplication"/>
        ///     <example>
        ///         <value>
        ///             <see cref="PublishedLunaApplication.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of published Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetRecommendedApplications")]
        public async Task<IActionResult> GetRecommendedApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "applications/{name}/recommended")]
            HttpRequest req,
            string name)
        {
            // TODO: rewrite the recommendation function. Now it's a mock function.
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetRecommendedApplications));

                try
                {
                    var applications = await _dbContext.PublishedLunaAppliations.
                        Where(x => x.IsEnabled).
                        Take(5).
                        Select(x => x.ToPublishedLunaApplication()).
                        ToListAsync();

                    return new OkObjectResult(applications);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetRecommendedApplications));
                }
            }
        }

        /// <summary>
        /// Get a published Luna application
        /// </summary>
        /// <group>Application</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applications/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="PublishedLunaApplication"/>
        ///     <example>
        ///         <value>
        ///             <see cref="PublishedLunaApplication.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of published Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetPublishedApplication")]
        public async Task<IActionResult> GetPublishedApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "applications/{name}")]
            HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetPublishedApplication));

                try
                {
                    var appDb = await _dbContext.PublishedLunaAppliations.
                        Where(x => x.UniqueName == name && x.IsEnabled).FirstOrDefaultAsync();

                    if (appDb == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, name));
                    }

                    return new OkObjectResult(appDb.ToPublishedLunaApplication());
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPublishedApplication));
                }
            }
        }

        /// <summary>
        /// Get swagger in JSON format for a published Luna application
        /// </summary>
        /// <group>Application</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applications/{name}/swagger</url>
        /// <param name="name" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationDetails"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationDetails.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of published Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetApplicationSwagger")]
        public async Task<IActionResult> GetApplicationSwagger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "applications/{name}/swagger")]
            HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetPublishedApplication));

                try
                {
                    var appDb = await _dbContext.PublishedLunaAppliations.
                        Where(x => x.UniqueName == name && x.IsEnabled).FirstOrDefaultAsync();

                    if (appDb == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, name));
                    }

                    return new OkObjectResult(appDb.ToPublishedLunaApplication().Details);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPublishedApplication));
                }
            }
        }

        #endregion

        #region subscription operations
        /// <summary>
        /// Create a subscription of a published Luna application
        /// </summary>
        /// <group>Subscription</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/subscriptions/{subscriptionName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="subscriptionName" required="true" cref="string" in="path">Name of the subscription</param>
        /// <param name="Luna-User-Id" required="true" cref="string" in="header">The user id of the creator</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscription"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscription.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateSubscription")]
        public async Task<IActionResult> CreateSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Put", Route = "applications/{appName}/subscriptions/{subscriptionName}")]
            HttpRequest req,
            string appName,
            string subscriptionName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateSubscription));

                try
                {
                    if (!await _dbContext.PublishedLunaAppliations.
                        AnyAsync(x => x.UniqueName == appName && x.IsEnabled))
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, appName));
                    }

                    if (await _dbContext.LunaApplicationSubscriptions.
                        AnyAsync(x => x.Status == LunaApplicationSubscriptionStatus.SUBCRIBED &&
                            x.ApplicationName == appName &&
                            x.SubscriptionName == subscriptionName))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.SUBSCIRPTION_ALREADY_EXIST, subscriptionName, appName));
                    }

                    var currentTime = DateTime.UtcNow;
                    var subscription = new LunaApplicationSubscriptionDB()
                    {
                        SubscriptionId = Guid.NewGuid(),
                        SubscriptionName = subscriptionName,
                        ApplicationName = appName,
                        Status = LunaApplicationSubscriptionStatus.SUBCRIBED,
                        Notes = string.Empty,
                        CreatedTime = currentTime,
                        LastUpdatedTime = currentTime
                    };

                    subscription.PrimaryKeySecretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.SUBSCRIPTION_KEY);
                    subscription.SecondaryKeySecretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.SUBSCRIPTION_KEY);
                    var primaryKey = Guid.NewGuid().ToString("N");
                    var secondaryKey = Guid.NewGuid().ToString("N");
                    await _keyVaultUtils.SetSecretAsync(subscription.PrimaryKeySecretName, primaryKey);
                    await _keyVaultUtils.SetSecretAsync(subscription.SecondaryKeySecretName, secondaryKey);

                    var owner = new LunaApplicationSubscriptionOwnerDB()
                    {
                        UserId = lunaHeaders.UserId,
                        UserName = lunaHeaders.UserName,
                        SubscriptionId = subscription.SubscriptionId,
                        CreatedTime = currentTime
                    };

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.LunaApplicationSubscriptions.Add(subscription);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.LunaApplicationSubscriptionOwners.Add(owner);
                        await _dbContext._SaveChangesAsync();

                        await _pubSubClient.PublishEventAsync(
                            LunaEventStoreType.SUBSCRIPTION_EVENT_STORE,
                            new CreateSubscriptionEventEntity()
                            {
                                SubscriptionId = subscription.SubscriptionId.ToString(),
                                EventContent = subscription.SubscriptionId.ToString()
                            },
                            lunaHeaders);

                        transaction.Commit();
                    }

                    var result = subscription.ToLunaApplicationSubscription();
                    result.BaseUrl = GetBaseUrl(appName);
                    result.PrimaryKey = primaryKey;
                    result.SecondaryKey = secondaryKey;

                    return new OkObjectResult(result);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateSubscription));
                }
            }
        }

        /// <summary>
        /// List all subscriptions of a published Luna application
        /// </summary>
        /// <group>Subscription</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/subscriptions</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="Luna-User-Id" required="true" cref="string" in="header">The user id of the caller</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="LunaApplicationSubscription"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscription.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListSubscriptions")]
        public async Task<IActionResult> ListSubscriptions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "applications/{appName}/subscriptions")]
            HttpRequest req,
            string appName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetSubscription));

                try
                {
                    var subscriptions = await _dbContext.LunaApplicationSubscriptions.
                        Include(x => x.Owners).
                        Where(x => x.ApplicationName == appName && x.Owners.Any(o => o.UserId == lunaHeaders.UserId)).
                        ToListAsync();

                    return new OkObjectResult(subscriptions);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetSubscription));
                }
            }
        }

        /// <summary>
        /// Get a subscription of a published Luna application
        /// </summary>
        /// <group>Subscription</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/subscriptions/{subscriptionNameOrId}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="Luna-User-Id" required="true" cref="string" in="header">The user id of the creator</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscription"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscription.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetSubscription")]
        public async Task<IActionResult> GetSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "applications/{appName}/subscriptions/{subscriptionNameOrId}")]
            HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetSubscription));

                try
                {
                    var subscriptionDb = await GetSubscriptionAndCheckOwner(appName, subscriptionNameOrId, lunaHeaders.UserId);

                    var result = subscriptionDb.ToLunaApplicationSubscription();
                    result.PrimaryKey = await _keyVaultUtils.GetSecretAsync(subscriptionDb.PrimaryKeySecretName);
                    result.SecondaryKey = await _keyVaultUtils.GetSecretAsync(subscriptionDb.SecondaryKeySecretName);
                    result.BaseUrl = GetBaseUrl(appName);

                    return new OkObjectResult(result);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetSubscription));
                }
            }
        }

        /// <summary>
        /// Delete a subscription of a published Luna application
        /// </summary>
        /// <group>Subscription</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/subscriptions/{subscriptionNameOrId}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="Luna-User-Id" required="true" cref="string" in="header">The user id of the creator</param>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteSubscription")]
        public async Task<IActionResult> DeleteSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Delete", Route = "applications/{appName}/subscriptions/{subscriptionNameOrId}")]
            HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetSubscription));

                try
                {
                    var subscriptionDb = await GetSubscriptionAndCheckOwner(appName, subscriptionNameOrId, lunaHeaders.UserId);

                    var currentTime = DateTime.UtcNow;
                    subscriptionDb.Status = LunaApplicationSubscriptionStatus.UNSUBSCRIBED;
                    subscriptionDb.LastUpdatedTime = currentTime;
                    subscriptionDb.UnsubscribedTime = currentTime;

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.LunaApplicationSubscriptions.Update(subscriptionDb);
                        await _dbContext._SaveChangesAsync();

                        await _pubSubClient.PublishEventAsync(
                            LunaEventStoreType.SUBSCRIPTION_EVENT_STORE,
                            new DeleteSubscriptionEventEntity()
                            {
                                SubscriptionId = subscriptionDb.SubscriptionId.ToString(),
                                EventContent = subscriptionDb.SubscriptionId.ToString()
                            },
                            lunaHeaders);

                        transaction.Commit();
                    }

                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetSubscription));
                }
            }
        }

        /// <summary>
        /// Update notes of a subscription
        /// </summary>
        /// <group>Subscription</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/subscriptions/{subscriptionNameOrId}/UpdateNotes</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="Luna-User-Id" required="true" cref="string" in="header">The user id of the creator</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationSubscriptionNotes"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionNotes.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription notes
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscriptionNotes"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionNotes.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription notes
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateSubscriptionNotes")]
        public async Task<IActionResult> UpdateSubscriptionNotes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "applications/{appName}/subscriptions/{subscriptionNameOrId}/UpdateNotes")]
            HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateSubscriptionNotes));

                try
                {
                    Guid subscriptionId = Guid.Empty;
                    Guid.TryParse(subscriptionNameOrId, out subscriptionId);

                    var subscriptionDb = await GetSubscriptionAndCheckOwner(appName, subscriptionNameOrId, lunaHeaders.UserId);

                    var notes = await HttpUtils.DeserializeRequestBodyAsync<LunaApplicationSubscriptionNotes>(req);

                    subscriptionDb.Notes = notes.Notes;

                    _dbContext.LunaApplicationSubscriptions.Update(subscriptionDb);
                    await _dbContext._SaveChangesAsync();

                    return new OkObjectResult(notes);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateSubscriptionNotes));
                }
            }
        }

        /// <summary>
        /// Add owner to a subscription
        /// </summary>
        /// <group>Subscription</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/subscriptions/{subscriptionNameOrId}/AddOwner</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="Luna-User-Id" required="true" cref="string" in="header">The user id of the creator</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationSubscriptionOwner"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionOwner.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription owner
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscriptionOwner"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionOwner.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription owner
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("AddSubscriptionOwner")]
        public async Task<IActionResult> AddSubscriptionOwner(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "applications/{appName}/subscriptions/{subscriptionNameOrId}/AddOwner")]
            HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddSubscriptionOwner));

                try
                {
                    Guid subscriptionId = Guid.Empty;
                    Guid.TryParse(subscriptionNameOrId, out subscriptionId);

                    var subscriptionDb = await GetSubscriptionAndCheckOwner(appName, subscriptionNameOrId, lunaHeaders.UserId);

                    var owner = await HttpUtils.DeserializeRequestBodyAsync<LunaApplicationSubscriptionOwner>(req);

                    if (await _dbContext.LunaApplicationSubscriptionOwners.
                        AnyAsync(x => x.SubscriptionId == subscriptionDb.SubscriptionId &&
                            x.UserId == owner.UserId))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.SUBSCIRPTION_OWNER_ALREADY_EXIST, owner.UserId, subscriptionNameOrId));
                    }

                    var ownerDb = new LunaApplicationSubscriptionOwnerDB()
                    {
                        UserId = owner.UserId,
                        UserName = owner.UserName,
                        SubscriptionId = subscriptionDb.SubscriptionId
                    };

                    _dbContext.LunaApplicationSubscriptionOwners.Add(ownerDb);
                    await _dbContext._SaveChangesAsync();

                    return new OkObjectResult(owner);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AddSubscriptionOwner));
                }
            }
        }

        /// <summary>
        /// Regenerate a subscription key
        /// </summary>
        /// <group>Subscription</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/subscriptions/{subscriptionNameOrId}/RegenerateKey</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="key-name" required="true" cref="string" in="query">The name of the key. Either PrimaryKey or SecondaryKey</param>
        /// <param name="Luna-User-Id" required="true" cref="string" in="header">The user id of the creator</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscriptionKeys"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionKeys.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription keys
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("RegenerateSubscriptionKey")]
        public async Task<IActionResult> RegenerateSubscriptionKey(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "applications/{appName}/subscriptions/{subscriptionNameOrId}/RegenerateKey")]
            HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RegenerateSubscriptionKey));

                try
                {
                    Guid subscriptionId = Guid.Empty;
                    Guid.TryParse(subscriptionNameOrId, out subscriptionId);

                    var subscriptionDb = await GetSubscriptionAndCheckOwner(appName, subscriptionNameOrId, lunaHeaders.UserId);

                    if (!req.Query.ContainsKey(GalleryServiceQueryParametersConstants.SUBCRIPTION_KEY_NAME_PARAM_NAME))
                    {
                        throw new LunaBadRequestUserException(string.Format(ErrorMessages.MISSING_QUERY_PARAMETER,
                            GalleryServiceQueryParametersConstants.SUBCRIPTION_KEY_NAME_PARAM_NAME),
                            UserErrorCode.MissingQueryParameter);
                    }

                    var keyName = req.Query[GalleryServiceQueryParametersConstants.SUBCRIPTION_KEY_NAME_PARAM_NAME].ToString();

                    if (keyName.Equals(GalleryServiceQueryParametersConstants.SUBCRIPTION_PRIMARY_KEY_VALUE))
                    {
                        var primaryKey = Guid.NewGuid().ToString("N");
                        await _keyVaultUtils.SetSecretAsync(subscriptionDb.PrimaryKeySecretName, primaryKey);
                    }
                    else if (keyName.Equals(GalleryServiceQueryParametersConstants.SUBCRIPTION_SECONDARY_KEY_VALUE))
                    {
                        var secondaryKey = Guid.NewGuid().ToString("N");
                        await _keyVaultUtils.SetSecretAsync(subscriptionDb.SecondaryKeySecretName, secondaryKey);
                    }
                    else
                    {
                        throw new LunaNotSupportedUserException(string.Format(ErrorMessages.KEY_NAME_NOT_SUPPORTED, keyName));
                    }

                    var keys = new LunaApplicationSubscriptionKeys()
                    {
                        PrimaryKey = await _keyVaultUtils.GetSecretAsync(subscriptionDb.PrimaryKeySecretName),
                        SecondaryKey = await _keyVaultUtils.GetSecretAsync(subscriptionDb.SecondaryKeySecretName)
                    };

                    await _pubSubClient.PublishEventAsync(
                        LunaEventStoreType.SUBSCRIPTION_EVENT_STORE,
                        new RegenerateSubscriptionKeyEventEntity()
                        {
                            SubscriptionId = subscriptionDb.SubscriptionId.ToString(),
                            EventContent = subscriptionDb.SubscriptionId.ToString()
                        },
                        lunaHeaders);

                    return new OkObjectResult(keys);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RegenerateSubscriptionKey));
                }
            }
        }

        /// <summary>
        /// Remove owner to a subscription
        /// </summary>
        /// <group>Subscription</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/subscriptions/{subscriptionNameOrId}/RemoveOwner</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="Luna-User-Id" required="true" cref="string" in="header">The user id of the creator</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationSubscriptionOwner"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionOwner.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription owner
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscriptionOwner"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionOwner.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription owner
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("RemoveSubscriptionOwner")]
        public async Task<IActionResult> RemoveSubscriptionOwner(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "applications/{appName}/subscriptions/{subscriptionNameOrId}/RemoveOwner")]
            HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemoveSubscriptionOwner));

                try
                {
                    Guid subscriptionId = Guid.Empty;
                    Guid.TryParse(subscriptionNameOrId, out subscriptionId);

                    var subscriptionDb = await GetSubscriptionAndCheckOwner(appName, subscriptionNameOrId, lunaHeaders.UserId);

                    var owner = await HttpUtils.DeserializeRequestBodyAsync<LunaApplicationSubscriptionOwner>(req);

                    if (owner == null)
                    {
                        throw new LunaBadRequestUserException(ErrorMessages.MISSING_REQUEST_BODY, UserErrorCode.InvalidParameter);
                    }

                    var ownerDb = await _dbContext.LunaApplicationSubscriptionOwners.
                        SingleOrDefaultAsync(x => x.SubscriptionId == subscriptionDb.SubscriptionId &&
                            x.UserId == owner.UserId);

                    if (ownerDb == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.SUBSCIRPTION_OWNER_DOES_NOT_EXIST, owner.UserId, subscriptionNameOrId));
                    }

                    _dbContext.LunaApplicationSubscriptionOwners.Remove(ownerDb);
                    await _dbContext._SaveChangesAsync();

                    return new OkObjectResult(owner);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RemoveSubscriptionOwner));
                }
            }
        }

        #endregion

        #region private methods
        private async Task<LunaApplicationSubscriptionDB> GetSubscriptionAndCheckOwner(string appName, string subscriptionNameOrId, string userId)
        {
            Guid subscriptionId = Guid.Empty;
            Guid.TryParse(subscriptionNameOrId, out subscriptionId);

            var subscriptionDb = await _dbContext.LunaApplicationSubscriptions.
                Include(x => x.Owners).
                SingleOrDefaultAsync(x => x.Status == LunaApplicationSubscriptionStatus.SUBCRIBED &&
                    x.ApplicationName == appName &&
                    (x.SubscriptionName == subscriptionNameOrId || x.SubscriptionId == subscriptionId));

            // TODO: should make the relationship in EF work...
            if (subscriptionDb == null ||
                !subscriptionDb.Owners.Any(x => x.UserId == userId))
            {
                throw new LunaNotFoundUserException(string.Format(ErrorMessages.SUBSCIRPTION_DOES_NOT_EXIST, subscriptionNameOrId));
            }

            return subscriptionDb;
        }

        private string GetBaseUrl(string appName)
        {
            return string.Format("{0}/{0}",
                        Environment.GetEnvironmentVariable(ROUTING_SERVICE_BASE_URL_CONFIG_NAME, EnvironmentVariableTarget.Process),
                        appName);
        }

        private async Task DeleteLunaApplication(string appName, long eventSequenceId)
        {
            var oldVersion = await _dbContext.PublishedLunaAppliations.
                Where(x => x.UniqueName == appName && x.IsEnabled).
                SingleOrDefaultAsync();

            var currentTime = DateTime.UtcNow;

            if (oldVersion != null)
            {
                oldVersion.IsEnabled = false;
                oldVersion.LastUpdatedTime = currentTime;
            }

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                if (oldVersion != null)
                {
                    _dbContext.PublishedLunaAppliations.Update(oldVersion);
                }

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            return;
        }

        private async Task CreateOrUpdateLunaApplication(string content, long eventSequenceId)
        {
            LunaApplication app = JsonConvert.DeserializeObject<LunaApplication>(content, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var oldVersion = await _dbContext.PublishedLunaAppliations.
                Where(x => x.UniqueName == app.Name && x.IsEnabled).
                SingleOrDefaultAsync();

            var currentTime = DateTime.UtcNow;

            if (oldVersion != null)
            {
                oldVersion.IsEnabled = false;
                oldVersion.LastUpdatedTime = currentTime;
            }

            var newVersion = new PublishedLunaAppliationDB(app, currentTime, eventSequenceId);

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.PublishedLunaAppliations.Add(newVersion);

                if (oldVersion != null)
                {
                    _dbContext.PublishedLunaAppliations.Update(oldVersion);
                }

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            return;
        }
        #endregion
    }
}
