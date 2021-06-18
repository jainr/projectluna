using Luna.Common.Utils;
using Luna.Provision.Clients;
using Luna.Provision.Data;
using Luna.Publish.Public.Client;
using Luna.PubSub.Public.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
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

        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<ProvisionServiceFunctions> _logger;
        private readonly IPubSubServiceClient _pubSubClient;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;
        private readonly ISwaggerClient _swaggerClient;

        public ProvisionServiceFunctions(ISqlDbContext dbContext, 
            ILogger<ProvisionServiceFunctions> logger, 
            IAzureKeyVaultUtils keyVaultUtils,
            IPubSubServiceClient pubSubClient,
            ISwaggerClient swaggerClient)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(dbContext));
            this._pubSubClient = pubSubClient ?? throw new ArgumentNullException(nameof(pubSubClient));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
            this._swaggerClient = swaggerClient ?? throw new ArgumentNullException(nameof(swaggerClient));
        }

        [FunctionName("ProcessApplicationEvents")]
        public async Task ProcessApplicationEvents([QueueTrigger("provision-processapplicationevents")] string myQueueItem)
        {
            // Get the last applied event id
            // If there's no record in the database, it will return the default value of long type 0
            var lastAppliedEventId = await _dbContext.LunaApplicationSwaggers.
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
                    var appName = ev.PartitionKey;
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

        [FunctionName("ProcessSubscriptionEvents")]
        public async Task ProcessSubscriptionEvents([QueueTrigger("provision-processsubscriptionevents")] string myQueueItem)
        {
            // Get the last applied event id
            // If there's no record in the database, it will return the default value of long type 0
            var lastAppliedEventId = await _dbContext.LunaApplicationSwaggers.
                OrderByDescending(x => x.LastAppliedEventId).
                Select(x => x.LastAppliedEventId).FirstOrDefaultAsync();

            var events = await _pubSubClient.ListEventsAsync(
                LunaEventStoreType.SUBSCRIPTION_EVENT_STORE,
                new LunaRequestHeaders(),
                eventsAfter: lastAppliedEventId);

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

    }
}
