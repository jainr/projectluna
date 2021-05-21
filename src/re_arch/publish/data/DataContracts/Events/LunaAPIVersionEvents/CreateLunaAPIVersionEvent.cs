using Luna.Publish.Data.Enums;
using Luna.Publish.Public.Client.DataContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.DataContracts.Events.LunaAPIVersionEvents
{
    public class CreateLunaAPIVersionEvent : BaseLunaAPIVersionEvent
    {
        public CreateLunaAPIVersionEvent()
            : base(PublishingEventType.CreateLunaAPIVersion)
        {

        }

        public BaseAPIVersionProp Properties { get; set; }
    }
}
