using Luna.Publish.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.DataContracts.Events.LunaApplicationEvents
{
    public class DeleteLunaApplicationEvent : BaseLunaApplicationEvent
    {
        public DeleteLunaApplicationEvent()
            : base(PublishingEventType.DeleteLunaApplication)
        {

        }
    }
}
