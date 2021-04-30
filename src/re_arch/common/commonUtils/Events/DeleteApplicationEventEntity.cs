using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.Events
{
    public class DeleteApplicationEventEntity : ApplicationEventEntity
    {
        public DeleteApplicationEventEntity()
        {
            
        }

        public DeleteApplicationEventEntity(string appName, string content) : base(appName, content)
        {
            EventType = LunaEventType.DELETE_APPLICATION_EVENT;
        }
    }
}
