using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class RegenerateApplicationMasterKeyEventEntity : ApplicationEventEntity
    {
        public RegenerateApplicationMasterKeyEventEntity()
        {
            EventType = LunaEventType.REGENERATE_APPLICATION_MASTER_KEY;
        }

        public RegenerateApplicationMasterKeyEventEntity(string appName, string content) : base(appName, content)
        {
            EventType = LunaEventType.REGENERATE_APPLICATION_MASTER_KEY;
        }
    }
}
