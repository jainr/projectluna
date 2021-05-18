using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public class EventStoreInfo
    {
        public EventStoreInfo(string name, string connectionString, DateTime? validThrough)
        {
            this.Name = name;
            this.ConnectionString = connectionString;
            this.ConnectionStringValidThroughUtc = validThrough;
            this.ValidEventTypes = new List<string>();
            this.EventSubscribers = new List<LunaEventSubscriber>();
        }

        public List<string> GetSubscriberQueueNames()
        {
            return this.EventSubscribers.Select(x => x.SubscriberQueueName).ToList();
        }

        public bool IsValidEventType(string eventName)
        {
            return ValidEventTypes.Contains(eventName);
        }

        public string Name { get; set; }

        public string ConnectionString { get; set; }

        public List<string> ValidEventTypes { get; set; }

        public List<LunaEventSubscriber> EventSubscribers { get; set; }

        public DateTime? ConnectionStringValidThroughUtc { get; set; }
    }
}
