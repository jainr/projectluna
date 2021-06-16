using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public abstract class BaseLunaApplicationEvent : BaseLunaPublishingEvent
    {
        public BaseLunaApplicationEvent(PublishingEventType type) : base(type)
        {

        }
    }
}
