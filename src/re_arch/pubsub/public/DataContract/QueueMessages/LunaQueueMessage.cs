using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.Public.Client
{
    public class LunaQueueMessage
    {
        public string EventType { get; set; }

        public string PartitionKey { get; set; }

        public long EventSequenceId { get; set; }

    }
}
