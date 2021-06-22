using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class BaseLunaAppEvent
    {
        public BaseLunaAppEvent(LunaAppEventType eventType)
        {
            this.EventType = eventType;
        }

        public string Name { get; set; }

        public LunaAppEventType EventType { get; }
    }
}
