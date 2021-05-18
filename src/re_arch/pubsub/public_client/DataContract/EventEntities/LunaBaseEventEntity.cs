using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public class LunaBaseEventEntity : TableEntity
    {
        public string EventType { get; set; }

        public string EventContent { get; set; }

        public DateTime CreatedTime { get; set; }

        public long EventSequenceId { get; set; }
    }
}
