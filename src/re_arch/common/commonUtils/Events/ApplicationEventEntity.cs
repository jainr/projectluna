using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.Events
{
    public abstract class ApplicationEventEntity : LunaBaseEventEntity
    {
        public ApplicationEventEntity()
        {

        }

        public ApplicationEventEntity(string appName, string content)
        {
            PartitionKey = appName;
            RowKey = Guid.NewGuid().ToString();

            ApplicationName = appName;
            EventId = RowKey;
            CreatedTime = DateTime.UtcNow;
            EventSequenceId = CreatedTime.Ticks;
            EventContent = content;
        }

        public string EventId { get; set; }

        public string ApplicationName { get; set; }

    }
}
