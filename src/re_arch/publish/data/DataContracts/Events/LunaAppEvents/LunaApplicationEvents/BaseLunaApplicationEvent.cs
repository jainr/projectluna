using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public abstract class BaseLunaApplicationEvent : BaseLunaAppEvent
    {
        public BaseLunaApplicationEvent(LunaAppEventType type) : base(type)
        {

        }
    }
}
