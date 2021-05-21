using Luna.Publish.Data.Enums;
using Luna.Publish.Public.Client.DataContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.DataContracts.Events.LunaAPIEvents
{
    public class DeleteLunaAPIEvent : BaseLunaAPIEvent
    {
        public DeleteLunaAPIEvent()
            : base(PublishingEventType.DeleteLunaAPI)
        {

        }
    }
}
