using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class DeleteLunaApplicationEvent : BaseLunaApplicationEvent
    {
        public DeleteLunaApplicationEvent()
            : base(PublishingEventType.DeleteLunaApplication)
        {

        }
    }
}
