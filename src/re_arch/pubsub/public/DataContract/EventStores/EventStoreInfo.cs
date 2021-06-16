using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class EventStoreInfo
    {
        public static string example = JsonConvert.SerializeObject(new EventStoreInfo(
            "ApplicationEvents",
            "Azure-Table-Storage-Connection-String-With-SaS-Key",
            new DateTime(637588561931352800))
        {
            ValidEventTypes = new List<string>(new string[] { 
                LunaEventType.DELETE_APPLICATION_EVENT, 
                LunaEventType.PUBLISH_APPLICATION_EVENT
            }),
        });

        public EventStoreInfo(string name, string connectionString, DateTime? validThrough)
        {
            this.Name = name;
            this.ConnectionString = connectionString;
            this.ConnectionStringValidThroughUtc = validThrough;
            this.ValidEventTypes = new List<string>();
            this.EventSubscribers = new List<LunaEventSubscriber>();
        }

        public bool IsValidEventType(string eventName)
        {
            return ValidEventTypes.Contains(eventName);
        }

        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "ConnectionString", Required = Required.Always)]
        public string ConnectionString { get; set; }

        [JsonProperty(PropertyName = "ValidEventTypes", Required = Required.Always)]
        public List<string> ValidEventTypes { get; set; }

        [JsonProperty(PropertyName = "ConnectionStringValidThroughUtc", Required = Required.Always)]
        public DateTime? ConnectionStringValidThroughUtc { get; set; }

        [JsonIgnore]
        public List<LunaEventSubscriber> EventSubscribers { get; set; }
    }
}
