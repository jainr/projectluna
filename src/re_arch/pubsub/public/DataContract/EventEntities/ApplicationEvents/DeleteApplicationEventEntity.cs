using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class DeleteApplicationEventEntity : ApplicationEventEntity
    {
        public DeleteApplicationEventEntity()
        {
            EventType = LunaEventType.DELETE_APPLICATION_EVENT;
        }

        public DeleteApplicationEventEntity(string appName, string content) : base(appName, content)
        {
            EventType = LunaEventType.DELETE_APPLICATION_EVENT;
        }
    }
}
