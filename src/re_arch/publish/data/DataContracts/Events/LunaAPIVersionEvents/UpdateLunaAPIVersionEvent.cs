using Luna.Publish.Data.Enums;
using Luna.Publish.Public.Client.DataContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.DataContracts.Events.LunaAPIVersionEvents
{
    public class UpdateLunaAPIVersionEvent : BaseLunaAPIVersionEvent
    {
        public UpdateLunaAPIVersionEvent()
            : base(PublishingEventType.UpdateLunaAPIVersion)
        {

        }

        public BaseAPIVersionProp Properties { get; set; }
    }
}
