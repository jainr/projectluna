using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.Events
{

    public class LunaEventStoreType
    {
        public const string APPLICATION_EVENT_STORE = "ApplicationEvents";
    }

    public class LunaEventType
    {
        public const string PUBLISH_APPLICATION_EVENT = "PUBLISH_APPLICATION_EVENT";
        public const string DELETE_APPLICATION_EVENT = "DELETE_APPLICATION_EVENT";
        public const string REGENERATE_APPLICATION_MASTER_KEY = "REGENERATE_APPLICATION_MASTER_KEY";
    }
}
