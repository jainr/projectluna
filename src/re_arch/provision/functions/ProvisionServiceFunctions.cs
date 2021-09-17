using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using Luna.Provision.Clients;
using Luna.Provision.Data;
using Luna.Publish.Public.Client;
using Luna.Marketplace.Public.Client;
using Luna.PubSub.Public.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Luna.Provision.Functions
{
    /// <summary>
    /// The service maintains all routings
    /// </summary>
    public class ProvisionServiceFunctions
    {
        private const string ROUTING_SERVICE_BASE_URL_CONFIG_NAME = "ROUTING_SERVICE_BASE_URL";

        private static ConcurrentDictionary<string, long> ApplicationsInProgress = new ConcurrentDictionary<string, long>();
        private static ConcurrentDictionary<string, long> SubscriptionsInProgress = new ConcurrentDictionary<string, long>();
        private static ConcurrentDictionary<string, long> MarketplaceOffersInProcess = new ConcurrentDictionary<string, long>();
        private static ConcurrentDictionary<string, long> MarketplaceSubsInProgress = new ConcurrentDictionary<string, long>();

        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<ProvisionServiceFunctions> _logger;
        private readonly IPubSubServiceClient _pubSubClient;
        private readonly IMarketplaceServiceClient _marketplaceClient;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;
        private readonly ISwaggerClient _swaggerClient;
        private readonly IProvisionStepClientFactory _provisionStepClientFactory;
        private readonly IProvisionFunctionsImpl _provisionFunctions;

        public ProvisionServiceFunctions(ISqlDbContext dbContext, 
            ILogger<ProvisionServiceFunctions> logger, 
            IAzureKeyVaultUtils keyVaultUtils,
            IPubSubServiceClient pubSubClient,
            IMarketplaceServiceClient marketplaceClient,
            ISwaggerClient swaggerClient,
            IProvisionStepClientFactory provisionStepClientFactory,
            IProvisionFunctionsImpl provisionFunctions)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(dbContext));
            this._pubSubClient = pubSubClient ?? throw new ArgumentNullException(nameof(pubSubClient));
            this._marketplaceClient = marketplaceClient ?? throw new ArgumentNullException(nameof(marketplaceClient));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
            this._swaggerClient = swaggerClient ?? throw new ArgumentNullException(nameof(swaggerClient));
            this._provisionStepClientFactory = provisionStepClientFactory ?? throw new ArgumentNullException(nameof(provisionStepClientFactory));
            this._provisionFunctions = provisionFunctions ?? throw new ArgumentNullException(nameof(provisionFunctions));
        }

        [FunctionName("ProcessApplicationEvents")]
        public async Task ProcessApplicationEvents([QueueTrigger("provision-processapplicationevents")] CloudQueueMessage myQueueItem)
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
                    queueMessage.YieldTo(ApplicationsInProgress, this._logger);

                    // Get the last applied event id
                    // If there's no record in the database, it will return the default value of long type 0
                    var lastAppliedEventId = await _dbContext.LunaApplicationSwaggers.
                        Where(x => x.ApplicationName == appName).
                        OrderByDescending(x => x.LastAppliedEventId).
                        Select(x => x.LastAppliedEventId).FirstOrDefaultAsync();

                    var events = await _pubSubClient.ListEventsAsync(
                        LunaEventStoreType.APPLICATION_EVENT_STORE,
                        new LunaRequestHeaders(),
                        eventsAfter: lastAppliedEventId,
                        partitionKey: appName);

                    foreach (var ev in events)
                    {
                        if (ev.EventType.Equals(LunaEventType.PUBLISH_APPLICATION_EVENT))
                        {
                            LunaApplication app = JsonConvert.DeserializeObject<LunaApplication>(ev.EventContent, new JsonSerializerSettings()
                            {
                                TypeNameHandling = TypeNameHandling.All
                            });

                            var swaggerContent = await this._swaggerClient.GenerateSwaggerAsync(app);

                            var swaggerDb = new LunaApplicationSwaggerDB()
                            {
                                ApplicationName = ev.PartitionKey,
                                SwaggerContent = swaggerContent,
                                SwaggerEventId = ev.EventSequenceId,
                                LastAppliedEventId = ev.EventSequenceId,
                                IsEnabled = true,
                                CreatedTime = DateTime.UtcNow
                            };

                            this._dbContext.LunaApplicationSwaggers.Add(swaggerDb);
                            await this._dbContext._SaveChangesAsync();
                        }
                        else
                        {
                            var isDeleted = ev.EventType.Equals(LunaEventType.DELETE_APPLICATION_EVENT);

                            var swaggerDb = await this._dbContext.LunaApplicationSwaggers.
                                Where(x => x.ApplicationName == appName).
                                OrderByDescending(x => x.LastAppliedEventId).FirstOrDefaultAsync();

                            if (swaggerDb != null)
                            {
                                swaggerDb.IsEnabled = !isDeleted;
                                swaggerDb.LastAppliedEventId = ev.EventSequenceId;

                                this._dbContext.LunaApplicationSwaggers.Update(swaggerDb);
                                await this._dbContext._SaveChangesAsync();
                            }
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
                    ApplicationsInProgress.TryRemove(appName, out value);

                    sw.Stop();
                    _logger.LogMethodEnd(nameof(this.ProcessApplicationEvents),
                        sw.ElapsedMilliseconds);
                }
            }
        }

        [FunctionName("ProcessSubscriptionEvents")]
        public async Task ProcessSubscriptionEvents([QueueTrigger("provision-processsubscriptionevents")] CloudQueueMessage myQueueItem)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string subId = "";
            using (_logger.BeginQueueTriggerNamedScope(myQueueItem))
            {
                try
                {
                    _logger.LogMethodBegin(nameof(this.ProcessSubscriptionEvents));

                    this._logger.LogDebug($"Received queue message {myQueueItem.AsString}.");

                    var queueMessage = JsonConvert.DeserializeObject<LunaQueueMessage>(myQueueItem.AsString);

                    subId = queueMessage.PartitionKey;
                    queueMessage.YieldTo(SubscriptionsInProgress, this._logger);

                }
                catch (Exception ex)
                {
                    ErrorUtils.HandleExceptions(ex, this._logger, string.Empty);
                }
                finally
                {
                    long value;
                    SubscriptionsInProgress.TryRemove(subId, out value);

                    sw.Stop();
                    _logger.LogMethodEnd(nameof(this.ProcessSubscriptionEvents),
                        sw.ElapsedMilliseconds);
                }
            }

        }


        [FunctionName("ProcessMarketplaceOfferEvents")]
        public async Task ProcessMarketplaceOfferEvents([QueueTrigger("provision-processmarketplaceofferevents")] CloudQueueMessage myQueueItem)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string offerId = "";
            using (_logger.BeginQueueTriggerNamedScope(myQueueItem))
            {
                try
                {
                    _logger.LogMethodBegin(nameof(this.ProcessMarketplaceOfferEvents));

                    this._logger.LogDebug($"Received queue message {myQueueItem.AsString}.");

                    var queueMessage = JsonConvert.DeserializeObject<LunaQueueMessage>(myQueueItem.AsString);

                    offerId = queueMessage.PartitionKey;
                    queueMessage.YieldTo(MarketplaceOffersInProcess, this._logger);

                    // Get the last applied event id
                    // If there's no record in the database, it will return the default value of long type 0
                    var lastAppliedEventId = await _dbContext.MarketplacePlans.
                        Where(x => x.OfferId == offerId).
                        OrderByDescending(x => x.CreatedByEventId).
                        Select(x => x.CreatedByEventId).FirstOrDefaultAsync();

                    var events = await _pubSubClient.ListEventsAsync(
                        LunaEventStoreType.AZURE_MARKETPLACE_OFFER_EVENT_STORE,
                        new LunaRequestHeaders(),
                        eventsAfter: lastAppliedEventId,
                        partitionKey: offerId);

                    foreach (var ev in events)
                    {
                        if (ev.EventType.Equals(LunaEventType.PUBLISH_AZURE_MARKETPLACE_OFFER))
                        {
                            MarketplaceOffer offer = JsonConvert.DeserializeObject<MarketplaceOffer>(ev.EventContent, new JsonSerializerSettings()
                            {
                                TypeNameHandling = TypeNameHandling.Auto
                            });

                            foreach (var plan in offer.Plans)
                            {
                                var planDb = new MarketplacePlanDB();
                                planDb.OfferId = offer.OfferId;
                                planDb.PlanId = plan.PlanId;
                                planDb.LunaApplicationName = plan.Properties.LunaApplicationName;
                                planDb.Mode = plan.Properties.Mode;
                                planDb.Properties = JsonConvert.SerializeObject(plan.Properties, new JsonSerializerSettings()
                                {
                                    TypeNameHandling = TypeNameHandling.All
                                });
                                var parameters = new List<MarketplaceParameter>();
                                parameters.AddRange(offer.Parameters);
                                parameters.AddRange(plan.Parameters);
                                planDb.Parameters = JsonConvert.SerializeObject(parameters, new JsonSerializerSettings()
                                {
                                    TypeNameHandling = TypeNameHandling.All
                                });
                                planDb.CreatedByEventId = ev.EventSequenceId;
                                planDb.ProvisioningStepsSecretName = offer.ProvisioningStepsSecretName;

                                _dbContext.MarketplacePlans.Add(planDb);
                            }

                            await _dbContext._SaveChangesAsync();
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
                    MarketplaceOffersInProcess.TryRemove(offerId, out value);

                    sw.Stop();
                    _logger.LogMethodEnd(nameof(this.ProcessMarketplaceOfferEvents),
                        sw.ElapsedMilliseconds);
                }
            }

        }

        [FunctionName("ProcessMarketplaceSubEvents")]
        public async Task ProcessMarketplaceSubEvents([QueueTrigger("provision-processmarketplacesubevents")] CloudQueueMessage myQueueItem)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string subId = "";
            using (_logger.BeginQueueTriggerNamedScope(myQueueItem))
            {
                try
                {
                    _logger.LogMethodBegin(nameof(this.ProcessMarketplaceSubEvents));

                    this._logger.LogDebug($"Received queue message {myQueueItem.AsString}.");

                    var queueMessage = JsonConvert.DeserializeObject<LunaQueueMessage>(myQueueItem.AsString);

                    subId = queueMessage.PartitionKey;
                    Guid subGuid = Guid.Parse(subId);

                    queueMessage.YieldTo(MarketplaceSubsInProgress, this._logger);

                    // Get the last applied event id
                    // If there's no record in the database, it will return the default value of long type 0
                    var lastAppliedEventId = await _dbContext.MarketplaceSubProvisionJobs.
                        Where(x => x.SubscriptionId == subGuid).
                        OrderByDescending(x => x.CreatedByEventId).
                        Select(x => x.CreatedByEventId).FirstOrDefaultAsync();

                    var events = await _pubSubClient.ListEventsAsync(
                        LunaEventStoreType.AZURE_MARKETPLACE_SUB_EVENT_STORE,
                        new LunaRequestHeaders(),
                        eventsAfter: lastAppliedEventId,
                        partitionKey: subId);

                    foreach (var ev in events)
                    {
                        if (ev.EventType.Equals(LunaEventType.CREATE_AZURE_MARKETPLACE_SUBSCRIPTION))
                        {
                            var sub = JsonConvert.DeserializeObject<MarketplaceSubscriptionEventContent>(ev.EventContent);

                            var plan = await _dbContext.MarketplacePlans.
                                SingleOrDefaultAsync(x => x.OfferId == sub.OfferId &&
                                x.PlanId == sub.PlanId && x.CreatedByEventId == sub.PlanCreatedByEventId);

                            if (plan == null)
                            {
                                throw new LunaServerException($"Plan {sub.PlanId} in offer {sub.OfferId} created by event {sub.PlanCreatedByEventId} does not exist.");
                            }

                            var currentTime = DateTime.UtcNow;

                            var jobDb = new MarketplaceSubProvisionJobDB()
                            {
                                SubscriptionId = sub.Id,
                                OfferId = sub.OfferId,
                                PlanId = sub.PlanId,
                                PlanCreatedByEventId = sub.PlanCreatedByEventId,
                                Mode = plan.Mode,
                                Status = ProvisionStatus.Queued.ToString(),
                                EventType = ev.EventType,
                                LunaApplicationName = plan.LunaApplicationName,
                                ProvisioningStepIndex = -1,
                                IsSynchronizedStep = false,
                                ProvisioningStepStatus = ProvisionStepStatus.NotStarted.ToString(),
                                ParametersSecretName = sub.ParametersSecretName,
                                ProvisionStepsSecretName = plan.ProvisioningStepsSecretName,
                                IsActive = false,
                                RetryCount = 0,
                                CreatedByEventId = ev.EventSequenceId,
                                CreatedTime = currentTime,
                                LastUpdatedTime = currentTime,
                            };

                            using (var transaction = await _dbContext.BeginTransactionAsync())
                            {
                                // Can have only one create job for a subscription
                                if (!await _dbContext.MarketplaceSubProvisionJobs.
                                    AnyAsync(x => x.SubscriptionId == sub.Id && x.EventType == ev.EventType))
                                {
                                    this._dbContext.MarketplaceSubProvisionJobs.Add(jobDb);
                                    await this._dbContext._SaveChangesAsync();
                                }

                                transaction.Commit();
                            }
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
                    MarketplaceSubsInProgress.TryRemove(subId, out value);

                    sw.Stop();
                    _logger.LogMethodEnd(nameof(this.ProcessMarketplaceSubEvents),
                        sw.ElapsedMilliseconds);
                }
            }
        }


        [FunctionName("ProcessProvisionJobs")]
        public async Task ProcessProvisionJobs([TimerTrigger("*/10 * * * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            // process active jobs
            var activeJobs = await _dbContext.MarketplaceSubProvisionJobs.Where(x => x.IsActive).ToListAsync();

            foreach (var job in activeJobs)
            {
                await this._provisionFunctions.ProcessActiveProvisioningJobStepAsync(job);
            }

            var queuedJobs = await _dbContext.MarketplaceSubProvisionJobs.
                OrderBy(x => x.CreatedByEventId).
                Where(x => x.Status == ProvisionStatus.Queued.ToString()).
                ToListAsync();

            // process queued jobs
            List<Guid> activedJobSubs = new List<Guid>();

            foreach (var job in queuedJobs)
            {
                // Only active one job at a time for a certian subscription
                if (!activeJobs.Any(x => x.SubscriptionId == job.SubscriptionId) && !activedJobSubs.Contains(job.SubscriptionId))
                {
                    var subId = await this._provisionFunctions.ActivateQueuedProvisioningJobAsync(job);
                    if (subId != null)
                    {
                        activedJobSubs.Add(subId.Value);
                    }
                }
            }

            _dbContext.MarketplaceSubProvisionJobs.UpdateRange(queuedJobs);
            await _dbContext._SaveChangesAsync();
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

        #region private methods

        #endregion

    }
}
