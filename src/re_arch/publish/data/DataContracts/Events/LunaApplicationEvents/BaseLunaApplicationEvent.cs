using Luna.Publish.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.DataContracts.Events.LunaApplicationEvents
{
    public abstract class BaseLunaApplicationEvent : BaseLunaPublishingEvent
    {
        public BaseLunaApplicationEvent(PublishingEventType type) : base(type)
        {

        }
    }
}
