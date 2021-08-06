using Luna.Common.Utils;
using Luna.PubSub.Clients;
using Luna.PubSub.Public.Client;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Luna.PubSub.Test
{
    [TestClass]
    public class PubSubFunctionTest
    {
        private LunaRequestHeaders _headers;
        private ILogger<PubSubFunctionsImpl> _logger;

        [TestInitialize]
        public void TestInitialize()
        {
            _headers = new LunaRequestHeaders();

            var mock = new Mock<ILogger<PubSubFunctionsImpl>>();
            this._logger = mock.Object;
        }

        [TestMethod]
        public async Task PublishAndListEvents()
        {
            var mock = new Mock<ILogger<EventStoreClient>>();
            var eventLogger = mock.Object;

            MockStorageUtils utils = new MockStorageUtils();
            IEventStoreClient client = new EventStoreClient(utils, eventLogger);
            var function = new PubSubFunctionsImpl(client, this._logger);

            var content = "test content";
            var appName = "my app";
            var applicationEvent = new DeleteApplicationEventEntity(appName, content);
            var testStartTicks = DateTime.UtcNow.Ticks;

            var ev = await function.PublishEventAsync(
                LunaEventStoreType.APPLICATION_EVENT_STORE,
                JsonConvert.SerializeObject(applicationEvent, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                }));

            var eventStore = await function.GetEventStoreInfoAsync(LunaEventStoreType.APPLICATION_EVENT_STORE);
            Assert.AreEqual(eventStore.ConnectionString, string.Format(MockStorageUtils.CONNECTION_STRING_FORMAT, LunaEventStoreType.APPLICATION_EVENT_STORE));

            Assert.IsTrue(utils.TableClient.ContainsKey(LunaEventStoreType.APPLICATION_EVENT_STORE));
            Assert.AreEqual(1, utils.TableClient[LunaEventStoreType.APPLICATION_EVENT_STORE].Count);
            Assert.AreEqual(eventStore.EventSubscribers.Count, utils.QueueClient.Count);

            foreach (var subscriber in eventStore.EventSubscribers)
            {
                Assert.IsTrue(utils.QueueClient.ContainsKey(subscriber.SubscriberQueueName));
                Assert.AreEqual(1, utils.QueueClient[subscriber.SubscriberQueueName].Count);
                Assert.IsTrue(utils.QueueClient[subscriber.SubscriberQueueName][0].Contains(LunaEventType.DELETE_APPLICATION_EVENT));
            }

            var events = await function.ListSortedEventsAsync(
                LunaEventStoreType.APPLICATION_EVENT_STORE,
                LunaEventType.DELETE_APPLICATION_EVENT,
                0,
                null);

            Assert.AreEqual(1, events.Count);

            events = await function.ListSortedEventsAsync(
                LunaEventStoreType.APPLICATION_EVENT_STORE,
                LunaEventType.DELETE_APPLICATION_EVENT,
                testStartTicks,
                null);

            Assert.AreEqual(1, events.Count);

            events = await function.ListSortedEventsAsync(
                LunaEventStoreType.APPLICATION_EVENT_STORE,
                LunaEventType.DELETE_APPLICATION_EVENT,
                0,
                appName);

            Assert.AreEqual(1, events.Count);


            // Get event with different event type
            events = await function.ListSortedEventsAsync(
                LunaEventStoreType.APPLICATION_EVENT_STORE,
                LunaEventType.PUBLISH_APPLICATION_EVENT,
                0,
                null);

            Assert.AreEqual(0, events.Count);

            // Get event after certain sequence id
            events = await function.ListSortedEventsAsync(
                LunaEventStoreType.APPLICATION_EVENT_STORE,
                LunaEventType.DELETE_APPLICATION_EVENT,
                DateTime.UtcNow.Ticks,
                null);

            Assert.AreEqual(0, events.Count);

            // Get event with different partition key
            events = await function.ListSortedEventsAsync(
                LunaEventStoreType.APPLICATION_EVENT_STORE,
                LunaEventType.DELETE_APPLICATION_EVENT,
                DateTime.UtcNow.Ticks,
                "wrong key");

            Assert.AreEqual(0, events.Count);
        }
    }
}
