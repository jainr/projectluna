using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Data
{
    public class LunaApplicationTelemetryItem: TableEntity
    {
        public LunaApplicationTelemetryItem(string publisher, string appName)
        {
            this.PartitionKey = publisher;
            this.RowKey = appName;
        }

        public LunaApplicationTelemetryItem(
            string publisher, 
            string appName,
            string displayName,
            DateTime createdTime,
            DateTime lastUpdatedTime) : this(publisher, appName)

        {
            this.Publisher = publisher;
            this.ApplicationName = appName;
            this.DisplayName = displayName;
            this.CreatedTime = createdTime;
            this.LastUpdatedTime = lastUpdatedTime;
        }

        public string Publisher { get; set; }

        public string ApplicationName { get; set; }

        public string DisplayName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
