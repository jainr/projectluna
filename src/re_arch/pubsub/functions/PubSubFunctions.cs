using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Common.Utils.LoggingUtils;
using Luna.Common.Utils.HttpUtils;
using Luna.Common.LoggingUtils;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.Azure.AzureKeyvaultUtils;
using Luna.PubSub.PublicClient;
using Luna.PubSub.Utils;

namespace Luna.PubSub.Functions
{
    /// <summary>
    /// The service maintains all Luna application, APIs and API versions
    /// </summary>
    public class PubSubFunctions
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly IAzureStorageUtils _storageUtils;
        private readonly ILogger<PubSubFunctions> _logger;

        public PubSubFunctions(
            IEventStoreClient eventStoreClient,
            IAzureStorageUtils storageUtils,
            ILogger<PubSubFunctions> logger)
        {
            this._eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
            this._storageUtils = storageUtils ?? throw new ArgumentNullException(nameof(storageUtils));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get event store connecton string
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="name">The name of the event store</param>
        /// <returns>The application</returns>
        [FunctionName("GetEventStoreConnectionString")]
        public async Task<IActionResult> GetEventStoreConnectionString(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "eventStores/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetEventStoreConnectionString));

                try
                {
                    var eventStore = await _eventStoreClient.GetEventStoreConnectionInfo(name);

                    return new OkObjectResult(eventStore);

                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetEventStoreConnectionString));
                }
            }
        }


        /// <summary>
        /// Get sorted events
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="name">The name of the event store</param>
        /// <returns>The application</returns>
        [FunctionName("GetSortedEvents")]
        public async Task<IActionResult> GetSortedEvents(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "eventStores/{name}/events")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetSortedEvents));

                try
                {
                    string eventType = null;
                    long eventsAfter = 0;
                    if (req.Query.ContainsKey(PubSubServiceQueryParameters.EVENT_TYPE))
                    {
                        eventType = req.Query[PubSubServiceQueryParameters.EVENT_TYPE].ToString();
                    }

                    if (req.Query.ContainsKey(PubSubServiceQueryParameters.EVENTS_AFTER))
                    {
                        var eventsAfterStr = req.Query[PubSubServiceQueryParameters.EVENTS_AFTER].ToString();
                        if (!long.TryParse(eventsAfterStr, out eventsAfter))
                        {
                            throw new LunaServerException($"EventsAfter query parameter should be in long data type.");
                        }
                    }

                    var events = await _eventStoreClient.ListEvents(name, eventType, eventsAfter);

                    return new OkObjectResult(events);

                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetSortedEvents));
                }
            }
        }

        /// <summary>
        /// publish a event to specified event store
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="name">The name of the event store</param>
        /// <param name="eventType">The event type</param>
        /// <returns>The application</returns>
        [FunctionName("PublishEvent")]
        public async Task<IActionResult> PublishEvent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "eventStores/{name}/events/publish")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.PublishEvent));

                try
                {
                    var content = await HttpUtils.GetRequestBodyAsync(req);

                    var ev = await _eventStoreClient.PublishEvent(name, content);

                    return new OkObjectResult(ev);

                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.PublishEvent));
                }
            }
        }
    }
}
