using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Data
{
    public class LunaUserTelemetryItem: TableEntity
    {
        public LunaUserTelemetryItem(string userId)
        {
            this.PartitionKey = "users";
            this.RowKey = userId;
        }

        public LunaUserTelemetryItem(
            string userId, 
            string userName,
            string displayName,
            DateTime createdTime,
            DateTime lastUpdatedTime) : this(userId)

        {
            this.UserName = userName;
            this.UserId = userId;
        }

        public string UserName { get; set; }

        public string UserId { get; set; }

    }
}
