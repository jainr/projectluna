using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class CreateLunaAPIEvent : BaseLunaAPIEvent
    {
        public CreateLunaAPIEvent()
            : base(PublishingEventType.CreateLunaAPI)
        {

        }

        public BaseLunaAPIProp Properties { get; set; }

    }
}
