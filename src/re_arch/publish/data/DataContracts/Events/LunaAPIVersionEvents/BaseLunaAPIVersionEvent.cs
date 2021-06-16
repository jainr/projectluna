using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public abstract class BaseLunaAPIVersionEvent : BaseLunaPublishingEvent
    {
        public BaseLunaAPIVersionEvent(PublishingEventType type) : base(type)
        {

        }

        public string ApplicationName { get; set; }

        public string APIName { get; set; }
    }
}
