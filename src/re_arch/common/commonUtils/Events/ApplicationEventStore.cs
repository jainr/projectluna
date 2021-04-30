using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.Events
{
    public class ApplicationEventStore : EventStore
    {
        public ApplicationEventStore(string connectionString, DateTime validThrough): 
            base(LunaEventStoreType.APPLICATION_EVENT_STORE, connectionString, validThrough)
        {
            ValidEventTypes.Add(LunaEventType.PUBLISH_APPLICATION_EVENT);
            ValidEventTypes.Add(LunaEventType.REGENERATE_APPLICATION_MASTER_KEY);
        }
    }
}
