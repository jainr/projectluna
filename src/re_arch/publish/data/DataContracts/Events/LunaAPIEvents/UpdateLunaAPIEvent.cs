using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
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
