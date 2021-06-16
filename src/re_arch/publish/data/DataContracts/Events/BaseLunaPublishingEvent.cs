using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class BaseLunaPublishingEvent
    {
        public BaseLunaPublishingEvent(PublishingEventType eventType)
        {
            this.EventType = eventType;
        }

        public string Name { get; set; }

        public PublishingEventType EventType { get; }
    }
}
