using Luna.Publish.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.DataContracts.Events
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
