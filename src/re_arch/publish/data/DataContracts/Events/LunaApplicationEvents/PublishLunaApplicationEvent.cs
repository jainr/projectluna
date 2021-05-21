using Luna.Publish.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.DataContracts.Events.LunaApplicationEvents
{
    public class PublishLunaApplicationEvent : BaseLunaApplicationEvent
    {
        public PublishLunaApplicationEvent()
            : base(PublishingEventType.PublishLunaApplication)
        {

        }

        public string PublishingComments { get; set; }
    }
}
