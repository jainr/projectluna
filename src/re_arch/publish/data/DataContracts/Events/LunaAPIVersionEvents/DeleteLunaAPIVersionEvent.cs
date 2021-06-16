using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class DeleteLunaAPIVersionEvent : BaseLunaAPIVersionEvent
    {
        public DeleteLunaAPIVersionEvent()
            : base(PublishingEventType.DeleteLunaAPIVersion)
        {

        }
    }
}
