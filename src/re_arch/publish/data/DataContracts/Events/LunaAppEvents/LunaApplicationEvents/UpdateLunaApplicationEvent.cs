using Luna.Publish.Public.Client;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Luna.Publish.Data
{
    /// <summary>
    /// The update Luna application event. 
    /// </summary>
    public class UpdateLunaApplicationEvent : BaseLunaApplicationEvent
    {
        public UpdateLunaApplicationEvent()
            : base(LunaAppEventType.UpdateLunaApplication)
        {

        }

        public LunaApplicationProp Properties { get; set; }

        public List<LunaApplicationTag> Tags { get; set; }

    }
}
