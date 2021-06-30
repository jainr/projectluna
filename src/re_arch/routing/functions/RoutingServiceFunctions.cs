using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Luna.Routing.Data;
using Luna.Routing.Clients;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Luna.Common.Utils;
using Luna.Publish.Public.Client;
using System.Collections.Generic;
using Luna.PubSub.Public.Client;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Diagnostics;
using Microsoft.Azure.Storage.Queue;
using System.Collections.Concurrent;
using System.Threading;

namespace Luna.Routing.Functions
{
    /// <summary>
    /// The service maintains all routings
    /// </summary>
    public class RoutingServiceFunctions
    {
        private const string API_VERSION_QUERY_PARAM_NAME = "api-version";
        private const int EVENT_WAITING_TIMEOUT_IN_MS = 30 * 1000;
        private const int EVENT_WAITING_INTERVAL_IN_MS = 100;

        private static ConcurrentDictionary<string, long> ApplicationsInProcess = new ConcurrentDictionary<string, long>();
        private static ConcurrentDictionary<string, long> SubscriptionsInProcess = new ConcurrentDictionary<string, long>();

        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<RoutingServiceFunctions> _logger;
        private readonly IMLServiceClientFactory _clientFactory;
        private readonly IPubSubServiceClient _pubSubClient;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;
        private readonly ISecretCacheClient _secretCacheClient;

        public RoutingServiceFunctions(ISqlDbContext dbContext, 
            ILogger<RoutingServiceFunctions> logger, 
            IMLServiceClientFactory clientFactory,
            IAzureKeyVaultUtils keyVaultUtils,
            IPubSubServiceClient pubSubClient,
            ISecretCacheClient secretCacheClient)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this._pubSubClient = pubSubClient ?? throw new ArgumentNullException(nameof(pubSubClient));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
            this._secretCacheClient = secretCacheClient ?? throw new ArgumentNullException(nameof(secretCacheClient));
        }

        private void YieldToExistingProcess(ConcurrentDictionary<string, long> inProcess, LunaQueueMessage queueMessage)
        {
            int waitTimeInMS = 0;

            while (!inProcess.TryAdd(queueMessage.PartitionKey, queueMessage.EventSequenceId))
            {
                this._logger.LogDebug($"Yield to existing process for {queueMessage.PartitionKey} with id {queueMessage.EventSequenceId}. Total wait time {waitTimeInMS} ms.");

                var eventSequenceId = inProcess[queueMessage.PartitionKey];

                if (queueMessage.EventSequenceId < eventSequenceId)
                {
                    this._logger.LogInformation($"The event {eventSequenceId} is being processed, skipping event {queueMessage.EventSequenceId}.");
                    return;
                }

                if (waitTimeInMS >= EVENT_WAITING_TIMEOUT_IN_MS)
                {
                    var errorMessage = $"The event with partition key {queueMessage.PartitionKey} and id {queueMessage.EventSequenceId} " +
                        $"has been waiting for more than {EVENT_WAITING_TIMEOUT_IN_MS} seconds. Abort the processing.";

                    throw new LunaServerException(errorMessage);
                }

                waitTimeInMS += EVENT_WAITING_INTERVAL_IN_MS;
                Thread.Sleep(EVENT_WAITING_INTERVAL_IN_MS);
            }
        }

        /// <summary>
        /// Process application events
        /// </summary>
        /// <param name="myQueueItem">The queue item</param>
        /// <returns></returns>
        [FunctionName("ProcessApplicationEvents")]
        public async Task ProcessApplicationEvents([QueueTrigger("routing-processapplicationevents")] CloudQueueMessage myQueueItem)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string appName = "";
            using (_logger.BeginQueueTriggerNamedScope(myQueueItem))
            {
                try
                {
                    _logger.LogMethodBegin(nameof(this.ProcessApplicationEvents));

                    this._logger.LogDebug($"Received queue message {myQueueItem.AsString}.");

                    var queueMessage = JsonConvert.DeserializeObject<LunaQueueMessage>(myQueueItem.AsString);

                    appName = queueMessage.PartitionKey;
                    YieldToExistingProcess(ApplicationsInProcess, queueMessage);

                    // Get the last applied event id
                    // If there's no record in the database, it will return the default value of long type 0
                    var lastAppliedEventId = await _dbContext.PublishedAPIVersions.
                        Where(x => x.ApplicationName == queueMessage.PartitionKey).
                        OrderByDescending(x => x.LastAppliedEventId).
                        Select(x => x.LastAppliedEventId).FirstOrDefaultAsync();

                    this._logger.LogDebug($"The last applied event id is {lastAppliedEventId}.");

                    // Only get events associated with specified application.
                    var events = await _pubSubClient.ListEventsAsync(
                        LunaEventStoreType.APPLICATION_EVENT_STORE,
                        new LunaRequestHeaders(),
                        eventsAfter: lastAppliedEventId,
                        partitionKey: queueMessage.PartitionKey);

                    this._logger.LogDebug($"{events.Count} events retreived with partition key {queueMessage.PartitionKey}.");

                    foreach (var ev in events)
                    {
                        if (ev.EventType.Equals(LunaEventType.PUBLISH_APPLICATION_EVENT))
                        {
                            var versions = await CreateOrUpdateLunaApplication(ev.EventContent, ev.EventSequenceId);

                            if (versions.Count > 0)
                            {
                                await _secretCacheClient.RefreshApplicationMasterKey(versions[0].PrimaryMasterKeySecretName);
                                await _secretCacheClient.RefreshApplicationMasterKey(versions[0].SecondaryMasterKeySecretName);
                                this._logger.LogDebug($"Application {ev.PartitionKey} created/updated. Master key refreshed in the cache.");
                            }
                            {
                                this._logger.LogWarning($"No version for application {ev.PartitionKey} found.");
                            }
                        }
                        else if (ev.EventType.Equals(LunaEventType.DELETE_APPLICATION_EVENT))
                        {
                            await DeleteLunaApplication(ev.PartitionKey, ev.EventSequenceId);
                            this._logger.LogDebug($"Application {ev.PartitionKey} deleted.");
                        }
                        else if (ev.EventType.Equals(LunaEventType.REGENERATE_APPLICATION_MASTER_KEY))
                        {
                            var apiVersion = await _dbContext.PublishedAPIVersions.
                                Where(x => x.ApplicationName == ev.PartitionKey && x.IsEnabled).
                                Take(1).
                                SingleOrDefaultAsync();

                            if (apiVersion != null)
                            {
                                await _secretCacheClient.RefreshApplicationMasterKey(apiVersion.PrimaryMasterKeySecretName);
                                await _secretCacheClient.RefreshApplicationMasterKey(apiVersion.SecondaryMasterKeySecretName);
                                await UpdateLunaApplication(ev.PartitionKey, ev.EventSequenceId);

                                this._logger.LogDebug($"Application {ev.PartitionKey} master key refreshed in the cache.");
                            }
                            else
                            {
                                this._logger.LogWarning($"No version for application {ev.PartitionKey} found.");
                            }
                        }
                        else
                        {
                            this._logger.LogError($"Unknown event type {ev.EventType}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorUtils.HandleExceptions(ex, this._logger, string.Empty);
                }
                finally
                {
                    long value;
                    ApplicationsInProcess.TryRemove(appName, out value);

                    sw.Stop();
                    _logger.LogMethodEnd(nameof(this.ProcessApplicationEvents),
                        sw.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>
        /// Process subscription events
        /// </summary>
        /// <param name="myQueueItem">The queue item</param>
        /// <returns></returns>
        [FunctionName("ProcessSubscriptionEvents")]
        public async Task ProcessSubscriptionEvents([QueueTrigger("routing-processsubscriptionevents")] CloudQueueMessage myQueueItem)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string subId = string.Empty;
            using (_logger.BeginQueueTriggerNamedScope(myQueueItem))
            {
                try
                {
                    var queueMessage = JsonConvert.DeserializeObject<LunaQueueMessage>(myQueueItem.AsString);

                    this._logger.LogDebug($"Received queue message {myQueueItem.AsString}.");

                    _logger.LogMethodBegin(nameof(this.ProcessSubscriptionEvents));

                    subId = queueMessage.PartitionKey;
                    YieldToExistingProcess(SubscriptionsInProcess, queueMessage);

                    // Get the last applied event id
                    // If there's no record in the database, it will return the default value of long type 0
                    var lastAppliedEventId = await _dbContext.SubscriptionEvents.
                        Where(x => x.SubscriptionId == queueMessage.PartitionKey).
                        OrderByDescending(x => x.LastAppliedEventId).
                        Select(x => x.LastAppliedEventId).FirstOrDefaultAsync();

                    // Add the record if lastAppliedEventId is 0
                    if (lastAppliedEventId == 0)
                    {
                        await _dbContext.SubscriptionEvents.AddAsync(new SubscriptionEventDB()
                        {
                            SubscriptionId = queueMessage.PartitionKey,
                            LastAppliedEventId = -1
                        });

                        await _dbContext._SaveChangesAsync();
                    }

                    this._logger.LogDebug($"The last applied event id is {lastAppliedEventId}.");

                    var events = await _pubSubClient.ListEventsAsync(
                        LunaEventStoreType.SUBSCRIPTION_EVENT_STORE,
                        new LunaRequestHeaders(),
                        eventsAfter: lastAppliedEventId,
                        partitionKey: queueMessage.PartitionKey);

                    this._logger.LogDebug($"{events.Count} events retreived with partition key {queueMessage.PartitionKey}.");

                    foreach (var ev in events)
                    {
                        if (ev.EventType.Equals(LunaEventType.CREATE_SUBSCRIPTION_EVENT) ||
                            ev.EventType.Equals(LunaEventType.REGENERATE_SUBSCRIPTION_KEY_EVENT))
                        {

                            var sub = await _dbContext.Subscriptions.SingleOrDefaultAsync(x => x.SubscriptionId == new Guid(ev.PartitionKey));

                            if (sub != null)
                            {
                                await _secretCacheClient.RefreshSubscriptionKey(sub.PrimaryKeySecretName);
                                await _secretCacheClient.RefreshSubscriptionKey(sub.SecondaryKeySecretName);
                                this._logger.LogDebug($"Subscription key for {sub.SubscriptionId} is refreshed.");
                            }
                            else
                            {
                                this._logger.LogDebug($"Can not find subscription {sub.SubscriptionId}. Might be deleted before the event was processed.");
                            }
                        }
                        else if (ev.EventType.Equals(LunaEventType.DELETE_SUBSCRIPTION_EVENT))
                        {
                            // Do nothing for now. The secret is invalid and will be unloaded after service restarted.
                            this._logger.LogDebug($"Delete subscription {ev.PartitionKey}.");
                        }

                        lastAppliedEventId = lastAppliedEventId < ev.EventSequenceId ? ev.EventSequenceId : lastAppliedEventId;
                    }

                    var processEvent = await _dbContext.SubscriptionEvents.
                        SingleOrDefaultAsync(x => x.SubscriptionId == queueMessage.PartitionKey);
                    processEvent.LastAppliedEventId = lastAppliedEventId;
                    _dbContext.SubscriptionEvents.Update(processEvent);
                    await _dbContext._SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    ErrorUtils.HandleExceptions(ex, this._logger, string.Empty);
                }
                finally
                {
                    long value;
                    SubscriptionsInProcess.TryRemove(subId, out value);

                    sw.Stop();
                    _logger.LogMethodEnd(nameof(this.ProcessSubscriptionEvents),
                        sw.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>
        /// Call an endpoint
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="operationName">The operation name</param>
        /// <returns></returns>
        [FunctionName("CallEndpoint")]
        public async Task<IActionResult> CallEndpoint(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{appName}/{apiName}/{operationName}")]
            HttpRequest req,
        string appName,
        string apiName,
        string operationName)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            var versionName = GetAPIVersion(req);
            var operationId = Guid.NewGuid().ToString();

            using (_logger.BeginRoutingNamedScope(appName, apiName, versionName, operationId, lunaHeaders, operationName))
            {
                IStatusCodeActionResult result = null;

                _logger.LogRoutingRequestBegin(nameof(this.CallEndpoint));

                try
                {
                    var apiVersion = await GetAndValidateAPIVersionInfo(versionName, appName, apiName);

                    lunaHeaders = await ValidateAccessKeys(lunaHeaders, apiVersion);

                    var versionProp = this.GetAPIVersionProperties(apiVersion.VersionProperties);

                    var input = await HttpUtils.GetRequestBodyAsync(req);

                    if (apiVersion.APIType.Equals(LunaAPIType.Realtime.ToString()))
                    {
                        var client = await _clientFactory.GetRealtimeEndpointClient(apiVersion.VersionType, versionProp);
                        var response = await client.CallRealtimeEndpoint(
                            operationName,
                            input,
                            versionProp,
                            lunaHeaders.GetPassThroughHeaders());
                        var content = await response.Content.ReadAsStringAsync();
                        // workaround the escape string issue from AML
                        //content = content.Replace("\\\"", "\"");
                        //content = content.Substring(1, content.Length - 2);
                        result = new ContentResult()
                        {
                            Content = content,
                            ContentType = response.Content.Headers.ContentType.MediaType,
                            StatusCode = (int)response.StatusCode
                        };
                    }
                    else if (apiVersion.APIType.Equals(LunaAPIType.Pipeline.ToString()))
                    {
                        var client = await _clientFactory.GetPipelineEndpointClient(apiVersion.VersionType, versionProp);
                        var response = await client.ExecutePipeline(
                            appName,
                            apiName,
                            apiVersion.VersionName,
                            operationName,
                            operationId,
                            input,
                            versionProp,
                            lunaHeaders.GetPassThroughHeaders());

                        result = new OkObjectResult(response);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    result = ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                    return result;
                }
                finally
                {
                    sw.Stop();
                    _logger.LogRoutingRequestEnd(nameof(this.CallEndpoint), 
                        result.StatusCode, 
                        lunaHeaders.SubscriptionId, 
                        sw.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>
        /// Get API metadata
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <returns></returns>
        [FunctionName("GetAPIMetadata")]
        public async Task<IActionResult> GetAPIMetadata(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{appName}/{apiName}/metadata")] HttpRequest req,
            string appName,
            string apiName)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginRoutingNamedScope(appName, apiName, "all", null, lunaHeaders))
            {
                IStatusCodeActionResult result = null;

                _logger.LogRoutingRequestBegin(nameof(this.GetAPIMetadata));

                try
                {
                    var versions = await _dbContext.PublishedAPIVersions.
                        Where(x => x.ApplicationName == appName && x.APIType == apiName && x.IsEnabled).
                        ToListAsync();

                    if (versions.Count > 0)
                    {
                        lunaHeaders = await ValidateAccessKeys(lunaHeaders, versions[0]);

                        var metadata = new LunaAPIMetadata(appName, apiName, versions[0].APIType);

                        foreach (var version in versions)
                        {
                            var versionMeta = new LunaAPIVersionMetadata(version.VersionName);
                            var prop = GetAPIVersionProperties(version.VersionProperties);

                            if (prop.GetType() == typeof(AzureMLRealtimeEndpointAPIVersionProp))
                            {
                                foreach(var endpoint in ((AzureMLRealtimeEndpointAPIVersionProp)prop).Endpoints)
                                {
                                    versionMeta.Operations.Add(endpoint.OperationName);
                                }
                            }
                            else if (prop.GetType() == typeof(AzureMLPipelineEndpointAPIVersionProp))
                            {
                                foreach (var endpoint in ((AzureMLPipelineEndpointAPIVersionProp)prop).Endpoints)
                                {
                                    versionMeta.Operations.Add(endpoint.OperationName);
                                }
                            }

                            metadata.Versions.Add(versionMeta);
                        }

                        result = new OkObjectResult(metadata);
                        return result;
                    }
                    else
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.API_DOES_NOT_EXIST, apiName, appName));
                    }
                }
                catch (Exception ex)
                {
                    result = ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                    return result;
                }
                finally
                {
                    sw.Stop();
                    _logger.LogRoutingRequestEnd(nameof(this.GetAPIMetadata),
                        result.StatusCode,
                        lunaHeaders.SubscriptionId,
                        sw.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>
        /// List all operations
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <returns></returns>
        [FunctionName("ListOperations")]
        public async Task<IActionResult> ListOperations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{appName}/{apiName}/operations")] HttpRequest req,
            string appName,
            string apiName)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            var versionName = GetAPIVersion(req);
            using (_logger.BeginRoutingNamedScope(appName, apiName, versionName, null, lunaHeaders))
            {
                IStatusCodeActionResult result = null;

                _logger.LogRoutingRequestBegin(nameof(this.ListOperations));

                try
                {
                    var apiVersion = await GetAndValidateAPIVersionInfo(versionName, appName, apiName);

                    lunaHeaders = await ValidateAccessKeys(lunaHeaders, apiVersion);

                    var versionProp = this.GetAPIVersionProperties(apiVersion.VersionProperties);

                    if (apiVersion.APIType.Equals(LunaAPIType.Pipeline.ToString()))
                    {
                        var client = await _clientFactory.GetPipelineEndpointClient(apiVersion.VersionType, versionProp);
                        var response = await client.ListOperations(
                            versionProp,
                            lunaHeaders.GetPassThroughHeaders());

                        result = new OkObjectResult(response);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    result = ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                    return result;
                }
                finally
                {
                    sw.Stop();
                    _logger.LogRoutingRequestEnd(nameof(this.ListOperations),
                        result.StatusCode,
                        lunaHeaders.SubscriptionId,
                        sw.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>
        /// Cancel an operation
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="operationId">The operation id</param>
        /// <returns></returns>
        [FunctionName("CancelOperation")]
        public async Task<IActionResult> CancelOperation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{appName}/{apiName}/operations/{operationId}/cancel")] HttpRequest req,
            string appName,
            string apiName,
            string operationId)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            var versionName = GetAPIVersion(req);
            using (_logger.BeginRoutingNamedScope(appName, apiName, versionName, operationId, lunaHeaders))
            {
                IStatusCodeActionResult result = null;

                _logger.LogRoutingRequestBegin(nameof(this.CancelOperation));

                try
                {
                    var apiVersion = await GetAndValidateAPIVersionInfo(versionName, appName, apiName);

                    lunaHeaders = await ValidateAccessKeys(lunaHeaders, apiVersion);

                    var versionProp = this.GetAPIVersionProperties(apiVersion.VersionProperties);

                    if (apiVersion.APIType.Equals(LunaAPIType.Pipeline.ToString()))
                    {
                        var client = await _clientFactory.GetPipelineEndpointClient(apiVersion.VersionType, versionProp);
                        await client.CancelOperation(
                            operationId,
                            versionProp,
                            lunaHeaders.GetPassThroughHeaders());

                        result = new NoContentResult();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    result = ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);

                    return result;
                }
                finally
                {
                    sw.Stop();
                    _logger.LogRoutingRequestEnd(nameof(this.CancelOperation),
                        result.StatusCode,
                        lunaHeaders.SubscriptionId,
                        sw.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>
        /// Get operation status
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="operationId">The operation id</param>
        /// <returns></returns>
        [FunctionName("GetOperationStatus")]
        public async Task<IActionResult> GetOperationStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{appName}/{apiName}/operations/{operationId}")] HttpRequest req,
            string appName,
            string apiName,
            string operationId)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            var versionName = GetAPIVersion(req);
            using (_logger.BeginRoutingNamedScope(appName, apiName, versionName, operationId, lunaHeaders))
            {
                IStatusCodeActionResult result = null;

                _logger.LogRoutingRequestBegin(nameof(this.GetOperationStatus));

                try
                {
                    var apiVersion = await GetAndValidateAPIVersionInfo(versionName, appName, apiName);

                    lunaHeaders = await ValidateAccessKeys(lunaHeaders, apiVersion);

                    var versionProp = this.GetAPIVersionProperties(apiVersion.VersionProperties);

                    if (apiVersion.APIType.Equals(LunaAPIType.Pipeline.ToString()))
                    {
                        var client = await _clientFactory.GetPipelineEndpointClient(apiVersion.VersionType, versionProp);
                        var response = await client.GetPipelineExecutionStatus(
                            operationId,
                            versionProp,
                            lunaHeaders.GetPassThroughHeaders());

                        result = new OkObjectResult(response);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    result = ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                    return result;
                }
                finally
                {
                    sw.Stop();
                    _logger.LogRoutingRequestEnd(nameof(this.GetOperationStatus),
                        result.StatusCode,
                        lunaHeaders.SubscriptionId,
                        sw.ElapsedMilliseconds);
                }
            }

        }

        /// <summary>
        /// Get operation output
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="operationId">The operation id</param>
        /// <returns>The operation output</returns>
        [FunctionName("GetOperationOutput")]
        public async Task<IActionResult> GetOperationOutput(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{appName}/{apiName}/operations/{operationId}/output")] HttpRequest req,
            string appName,
            string apiName,
            string operationId)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            var versionName = GetAPIVersion(req);
            using (_logger.BeginRoutingNamedScope(appName, apiName, versionName, operationId, lunaHeaders))
            {
                IStatusCodeActionResult result = null;

                _logger.LogRoutingRequestBegin(nameof(this.GetOperationOutput));

                try
                {
                    var apiVersion = await GetAndValidateAPIVersionInfo(versionName, appName, apiName);

                    lunaHeaders = await ValidateAccessKeys(lunaHeaders, apiVersion);

                    var versionProp = this.GetAPIVersionProperties(apiVersion.VersionProperties);

                    if (apiVersion.APIType.Equals(LunaAPIType.Pipeline.ToString()))
                    {
                        var client = await _clientFactory.GetPipelineEndpointClient(apiVersion.VersionType, versionProp);
                        var response = await client.GetPipelineExecutionJsonOutput(
                            operationId,
                            versionProp,
                            lunaHeaders.GetPassThroughHeaders());

                        result = new OkObjectResult(response);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    result = ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                    return result;
                }
                finally
                {
                    sw.Stop();
                    _logger.LogRoutingRequestEnd(nameof(this.GetOperationOutput),
                        result.StatusCode,
                        lunaHeaders.SubscriptionId,
                        sw.ElapsedMilliseconds);
                }
            }
        }

        #region Private methods

        private async Task<PublishedAPIVersionDB> GetAndValidateAPIVersionInfo(string versionName, string appName, string apiName)
        {
            if (versionName == null)
            {
                throw new LunaBadRequestUserException(ErrorMessages.MISSING_QUERY_PARAMETER, UserErrorCode.MissingQueryParameter);
            }
            var apiVersion = await _dbContext.PublishedAPIVersions.FirstOrDefaultAsync(
                x => x.IsEnabled &&
                x.ApplicationName == appName &&
                x.APIName == apiName &&
                x.VersionName == versionName);

            if (apiVersion == null)
            {
                throw new LunaNotFoundUserException(
                    string.Format(ErrorMessages.API_VERSION_DOES_NOT_EXIST, versionName, apiName));
            }

            return apiVersion;
        }

        private async Task<LunaRequestHeaders> ValidateAccessKeys(LunaRequestHeaders lunaHeaders, PublishedAPIVersionDB apiVersion)
        {
            if (!_secretCacheClient.SecretCache.IsInitialized)
            {
                var subscriptions = await _dbContext.Subscriptions.ToListAsync();
                var apiVersions = await _dbContext.PublishedAPIVersions.Where(x => x.IsEnabled).ToListAsync();
                var partnerService = await _dbContext.PartnerServices.ToListAsync();
                await _secretCacheClient.Init(subscriptions, apiVersions, partnerService);
            }

            if (string.IsNullOrEmpty(lunaHeaders.LunaApplicationMasterKey))
            {
                if (string.IsNullOrEmpty(lunaHeaders.LunaSubscriptionKey))
                {
                    throw new LunaUnauthorizedUserException("The master key or subscription key is required");
                }

                if (_secretCacheClient.SecretCache.SubscriptionKeys.ContainsKey(lunaHeaders.LunaSubscriptionKey))
                {
                    var secretName = _secretCacheClient.SecretCache.SubscriptionKeys[lunaHeaders.LunaSubscriptionKey].SecretName;
                    var sub = await _dbContext.Subscriptions.
                        Where(x => (x.PrimaryKeySecretName == secretName || x.SecondaryKeySecretName == secretName) &&
                        x.ApplicationName == apiVersion.ApplicationName).
                        SingleOrDefaultAsync();

                    if (sub == null)
                    {
                        throw new LunaUnauthorizedUserException(ErrorMessages.INVALID_KEY);
                    }

                    lunaHeaders.SubscriptionId = sub.SubscriptionId.ToString();

                    if (string.IsNullOrEmpty(lunaHeaders.UserId))
                    {
                        lunaHeaders.UserId = sub.SubscriptionId.ToString();
                    }
                }
                else
                {
                    throw new LunaUnauthorizedUserException(ErrorMessages.INVALID_KEY);
                }

            }
            else
            {
                if (_secretCacheClient.SecretCache.ApplicationMasterKeys.ContainsKey(lunaHeaders.LunaApplicationMasterKey))
                {
                    var itemCache = _secretCacheClient.SecretCache.ApplicationMasterKeys[lunaHeaders.LunaApplicationMasterKey];
                    if (itemCache.SecretName.Equals(apiVersion.PrimaryMasterKeySecretName) ||
                        itemCache.SecretName.Equals(apiVersion.SecondaryMasterKeySecretName))
                    {

                        if (string.IsNullOrEmpty(lunaHeaders.SubscriptionId))
                        {
                            lunaHeaders.SubscriptionId = "master";
                        }

                        if (string.IsNullOrEmpty(lunaHeaders.UserId))
                        {
                            lunaHeaders.UserId = "master";
                        }
                    }
                }
                else
                {
                    throw new LunaUnauthorizedUserException(ErrorMessages.INVALID_KEY);
                }
            }

            return lunaHeaders;
        }

        private BaseAPIVersionProp GetAPIVersionProperties(string versionProperties)
        {
            return JsonConvert.DeserializeObject<BaseAPIVersionProp>(versionProperties, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        private string GetAPIVersion(HttpRequest req)
        {
            if (req.Query.ContainsKey(API_VERSION_QUERY_PARAM_NAME))
            {
                return req.Query[API_VERSION_QUERY_PARAM_NAME];
            }
            else
            {
                return null;
            }
        }

        private async Task DeleteLunaApplication(string name, long eventSequenceId)
        {
            var oldVersions = await _dbContext.PublishedAPIVersions.Where(x => x.ApplicationName == name).ToListAsync();
            var currentTime = DateTime.UtcNow;

            foreach (var oldVersion in oldVersions)
            {
                oldVersion.LastAppliedEventId = eventSequenceId;
                oldVersion.LastUpdatedTime = currentTime;
                oldVersion.IsEnabled = false;
            }

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                if (oldVersions.Count > 0)
                {
                    _dbContext.PublishedAPIVersions.UpdateRange(oldVersions);
                }

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            return;
        }

        private async Task UpdateLunaApplication(string name, long eventSequenceId)
        {
            var versions = await _dbContext.PublishedAPIVersions.Where(x => x.ApplicationName == name).ToListAsync();
            var currentTime = DateTime.UtcNow;

            foreach (var oldVersion in versions)
            {
                oldVersion.LastAppliedEventId = eventSequenceId;
                oldVersion.LastUpdatedTime = currentTime;
            }

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                if (versions.Count > 0)
                {
                    _dbContext.PublishedAPIVersions.UpdateRange(versions);
                }

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            return;
        }

        private async Task<List<PublishedAPIVersionDB>> CreateOrUpdateLunaApplication(string content, long eventSequenceId)
        {
            LunaApplication app = JsonConvert.DeserializeObject<LunaApplication>(content, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            // If this function is triggered more than one time on the same event, there will be duplicated API version created
            // We used FirstOrDefault when getting API version from database to mitigate this issue.

            var oldVersions = await _dbContext.PublishedAPIVersions.
                Where(x => x.ApplicationName == app.Name && x.IsEnabled).
                ToListAsync();

            var currentTime = DateTime.UtcNow;

            foreach (var oldVersion in oldVersions)
            {
                oldVersion.LastUpdatedTime = currentTime;
                oldVersion.IsEnabled = false;
            }

            var newVersions = new List<PublishedAPIVersionDB>();

            foreach (var api in app.APIs)
            {
                foreach (var version in api.Versions)
                {
                    var prop = JsonConvert.SerializeObject(version.Properties, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

                    var ver = new PublishedAPIVersionDB()
                    {
                        ApplicationName = app.Name,
                        APIName = api.Name,
                        APIType = api.Properties.Type,
                        VersionName = version.Name,
                        VersionType = version.Properties.Type,
                        VersionProperties = prop,
                        LastAppliedEventId = eventSequenceId,
                        PrimaryMasterKeySecretName = app.Properties.PrimaryMasterKeySecretName,
                        SecondaryMasterKeySecretName = app.Properties.SecondaryMasterKeySecretName,
                        IsEnabled = true,
                        CreatedTime = currentTime,
                        LastUpdatedTime = currentTime
                    };
                    newVersions.Add(ver);
                }
            }

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                if (newVersions.Count > 0)
                {
                    _dbContext.PublishedAPIVersions.AddRange(newVersions);
                }

                if (oldVersions.Count > 0)
                {
                    _dbContext.PublishedAPIVersions.UpdateRange(oldVersions);
                }

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            return newVersions;
        }
        #endregion
    }
}
