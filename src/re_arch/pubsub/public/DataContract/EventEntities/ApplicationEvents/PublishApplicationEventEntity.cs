using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class PublishApplicationEventEntity : ApplicationEventEntity
    {
        public PublishApplicationEventEntity()
        {
            EventType = LunaEventType.PUBLISH_APPLICATION_EVENT;
        }

        public PublishApplicationEventEntity(string appName, string content) : base(appName, content)
        {
            EventType = LunaEventType.PUBLISH_APPLICATION_EVENT;
        }
    }
}
