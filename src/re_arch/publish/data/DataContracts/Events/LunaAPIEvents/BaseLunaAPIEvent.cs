using Luna.Publish.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.DataContracts.Events.LunaAPIEvents
{
    public abstract class BaseLunaAPIEvent : BaseLunaPublishingEvent
    {
        public BaseLunaAPIEvent(PublishingEventType type) : base(type)
        {

        }

        public string ApplicationName { get; set; }
    }
}
