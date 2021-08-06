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
using Luna.Common.Utils;
using Luna.PubSub.Public.Client;
using Luna.PubSub.Clients;

namespace Luna.PubSub.Functions
{
    /// <summary>
    /// The service maintains all Luna application, APIs and API versions
    /// </summary>
    public class PubSubFunctions
    {
        private readonly IPubSubFunctionsImpl _functionImpl;
        private readonly ILogger<PubSubFunctions> _logger;

        public PubSubFunctions(
            IPubSubFunctionsImpl functionImpl,
            ILogger<PubSubFunctions> logger)
        {
            this._functionImpl = functionImpl ?? throw new ArgumentNullException(nameof(functionImpl));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get event store connection string
        /// </summary>
        /// <group>EventStore</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/eventStores/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the event store</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="EventStoreInfo"/>
        ///     <example>
        ///         <value>
        ///             <see cref="EventStoreInfo.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of event store info
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetEventStoreInfo")]
        public async Task<IActionResult> GetEventStoreInfo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "eventStores/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetEventStoreInfo));

                try
                {
                    var eventStore = await _functionImpl.GetEventStoreInfoAsync(name);

                    return new OkObjectResult(eventStore);

                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetEventStoreInfo));
                }
            }
        }


        /// <summary>
        /// Get events sorted by published time from specified event store
        /// </summary>
        /// <group>Event</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/eventStores/{name}/events</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the event store</param>
        /// <param name="event-type" required="false" cref="string" in="query">The event type to query</param>
        /// <param name="event-after" required="false" cref="long" in="query">If specified, only get events generated after the specified event id</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="LunaBaseEventEntity"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaBaseEventEntity.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of event entity
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListSortedEvents")]
        public async Task<IActionResult> ListSortedEvents(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "eventStores/{name}/events")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListSortedEvents));

                try
                {
                    string eventType = null;
                    long eventsAfter = 0;
                    string partitionKey = null;
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

                    if (req.Query.ContainsKey(PubSubServiceQueryParameters.PARTITION_KEY))
                    {
                        partitionKey = req.Query[PubSubServiceQueryParameters.PARTITION_KEY].ToString();
                    }

                    var events = await _functionImpl.ListSortedEventsAsync(name, eventType, eventsAfter, partitionKey);

                    return new OkObjectResult(events);

                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListSortedEvents));
                }
            }
        }

        /// <summary>
        /// Publish an event to the specified event store
        /// </summary>
        /// <group>Event</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/eventStores/{name}/events/publish</url>
        /// <param name="name" required="true" cref="string" in="path">Name of event store</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaBaseEventEntity"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaBaseEventEntity.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of event entity
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaBaseEventEntity"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaBaseEventEntity.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of event entity
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
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

                    var ev = await _functionImpl.PublishEventAsync(name, content);

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
