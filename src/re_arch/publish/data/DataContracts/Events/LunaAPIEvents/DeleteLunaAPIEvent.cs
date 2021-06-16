using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class DeleteLunaAPIEvent : BaseLunaAPIEvent
    {
        public DeleteLunaAPIEvent()
            : base(PublishingEventType.DeleteLunaAPI)
        {

        }
    }
}
