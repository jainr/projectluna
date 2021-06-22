using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public abstract class BaseLunaAPIEvent : BaseLunaAppEvent
    {
        public BaseLunaAPIEvent(LunaAppEventType type) : base(type)
        {

        }

        public string ApplicationName { get; set; }
    }
}
