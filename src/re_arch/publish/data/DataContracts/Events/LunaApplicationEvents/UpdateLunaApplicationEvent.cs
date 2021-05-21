using Luna.Publish.Data.Enums;
using Luna.Publish.Public.Client.DataContract;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Luna.Publish.Data.DataContracts.Events.LunaApplicationEvents
{
    /// <summary>
    /// The update Luna application event. 
    /// </summary>
    public class UpdateLunaApplicationEvent : BaseLunaApplicationEvent
    {
        public UpdateLunaApplicationEvent()
            : base(PublishingEventType.UpdateLunaApplication)
        {

        }

        public LunaApplicationProp Properties { get; set; }

        public List<LunaApplicationTag> Tags { get; set; }

    }
}
