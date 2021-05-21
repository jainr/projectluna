using Luna.Publish.Data.DataContracts.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.Entities
{
    /// <summary>
    /// The database entity for publishing events
    /// </summary>
    public class PublishingEventDB
    {
        [JsonIgnore]
        public long Id { get; set; }

        public Guid EventId { get; set; }

        public string EventType { get; set; }

        public string ResourceName { get; set; }

        public string EventContent { get; set; }

        public string CreatedBy { get; set; }

        public string Tags { get; set; }

        public DateTime CreatedTime { get; set; }

        public BaseLunaPublishingEvent GetEventObject()
        {
            BaseLunaPublishingEvent obj = (BaseLunaPublishingEvent)JsonConvert.DeserializeObject(this.EventContent, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return obj;
        }

    }
}
