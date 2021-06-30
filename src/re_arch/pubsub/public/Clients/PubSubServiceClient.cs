using Luna.Common.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.PubSub.Public.Client
{
    public class PubSubServiceClient : RestClient, IPubSubServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PubSubServiceClient> _logger;
        private readonly PubSubServiceClientConfiguration _config;

        [ActivatorUtilitiesConstructor]
        public PubSubServiceClient(IOptionsMonitor<PubSubServiceClientConfiguration> option,
            HttpClient httpClient,
            ILogger<PubSubServiceClient> logger) :
            base(option, httpClient, logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._config = option.CurrentValue ?? throw new ArgumentNullException(nameof(option.CurrentValue));
        }

        /// <summary>
        /// Get event store connection string
        /// </summary>
        /// <param name="name">The event store name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The event store connection string and other info</returns>
        public async Task<EventStoreInfo> GetEventStoreConnectionStringAsync(string name, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"eventStores/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<EventStoreInfo>(response);
        }

        /// <summary>
        /// List events in the specified event store
        /// </summary>
        /// <param name="eventStoreName">The event store name</param>
        /// <param name="headers">The luna request header</param>
        /// <param name="eventType"></param>
        /// <param name="eventsAfter"></param>
        /// <param name="partitionKey"></param>
        /// <returns>The event list</returns>
        public async Task<List<LunaBaseEventEntity>> ListEventsAsync(
            string eventStoreName, 
            LunaRequestHeaders headers, 
            string eventType = null, 
            long eventsAfter = 0,
            string partitionKey = null)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var url = this._config.ServiceBaseUrl + $"eventStores/{eventStoreName}/events?{PubSubServiceQueryParameters.EVENTS_AFTER}={eventsAfter}";

            if (eventType != null)
            {
                url = url + $"&{PubSubServiceQueryParameters.EVENT_TYPE}=" + eventType;
            }

            if (partitionKey != null)
            {
                url = url + $"&{PubSubServiceQueryParameters.PARTITION_KEY}=" + partitionKey;
            }

            var uri = new Uri(url);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<List<LunaBaseEventEntity>>(response);
        }

        /// <summary>
        /// Publish a event to the specified event store
        /// </summary>
        /// <param name="eventStoreName">The event store name</param>
        /// <param name="ev">The event content</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The published event</returns>
        public async Task<LunaBaseEventEntity> PublishEventAsync(string eventStoreName, LunaBaseEventEntity ev, LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"eventStores/{eventStoreName}/events/publish");

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, content, headers);

            return await GetResponseObject<LunaBaseEventEntity>(response);
        }

        private async Task<T> GetResponseObject<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            return obj;
        }
    }
}
