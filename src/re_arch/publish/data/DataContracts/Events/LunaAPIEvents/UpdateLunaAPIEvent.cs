using Luna.Publish.Data.Enums;
using Luna.Publish.Public.Client.DataContract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data.DataContracts.Events.LunaAPIEvents
{
    public class UpdateLunaAPIEvent : BaseLunaAPIEvent
    {
        public UpdateLunaAPIEvent()
            : base(PublishingEventType.UpdateLunaAPI)
        {

        }

        public BaseLunaAPIProp Properties { get; set; }

    }
}
