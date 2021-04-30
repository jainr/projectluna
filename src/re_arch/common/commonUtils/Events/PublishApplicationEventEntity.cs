using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.Events
{
    public class PublishApplicationEventEntity : ApplicationEventEntity
    {
        public PublishApplicationEventEntity()
        {
            
        }

        public PublishApplicationEventEntity(string appName, string content) : base(appName, content)
        {
            EventType = LunaEventType.PUBLISH_APPLICATION_EVENT;
        }
    }
}
