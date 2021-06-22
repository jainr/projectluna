using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public abstract class BaseLunaAPIVersionEvent : BaseLunaAppEvent
    {
        public BaseLunaAPIVersionEvent(LunaAppEventType type) : base(type)
        {

        }

        public string ApplicationName { get; set; }

        public string APIName { get; set; }
    }
}
