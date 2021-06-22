using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class PublishLunaApplicationEvent : BaseLunaApplicationEvent
    {
        public PublishLunaApplicationEvent()
            : base(LunaAppEventType.PublishLunaApplication)
        {

        }

        public string PublishingComments { get; set; }
    }
}
