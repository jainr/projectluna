using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public class LunaBaseEventEntity : TableEntity
    {
        public static string example = JsonConvert.SerializeObject(new LunaBaseEventEntity()
        {
            EventType = LunaEventType.PUBLISH_APPLICATION_EVENT,
            EventContent = "{}",
            CreatedTime = new DateTime(637588561931352800),
            EventSequenceId = 637588561931352800
        });

        [JsonProperty(PropertyName = "EventType", Required = Required.Always)]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "EventContent", Required = Required.Always)]
        public string EventContent { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "EventSequenceId", Required = Required.Default)]
        public long EventSequenceId { get; set; }
    }
}
