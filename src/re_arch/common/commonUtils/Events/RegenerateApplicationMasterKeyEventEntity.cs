using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.Events
{
    public class RegenerateApplicationMasterKeyEventEntity : ApplicationEventEntity
    {
        public RegenerateApplicationMasterKeyEventEntity()
        {

        }

        public RegenerateApplicationMasterKeyEventEntity(string appName, string content) : base(appName, content)
        {
            EventType = LunaEventType.REGENERATE_APPLICATION_MASTER_KEY;
        }
    }
}
