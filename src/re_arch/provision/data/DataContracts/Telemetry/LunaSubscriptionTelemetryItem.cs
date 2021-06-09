using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Data
{
    public class LunaSubscriptionTelemetryItem: TableEntity
    {
        public LunaSubscriptionTelemetryItem(string appName, string subscriptionId)
        {
            this.PartitionKey = appName;
            this.RowKey = subscriptionId;
        }

        public LunaSubscriptionTelemetryItem(
            string appName, 
            string subscriptionId,
            string createdBy,
            string status,
            DateTime? unsubscribedTime,
            DateTime createdTime,
            DateTime lastUpdatedTime) : this(appName, subscriptionId)

        {
            this.SubscriptionId = subscriptionId;
            this.ApplicationName = appName;
            this.Status = status;
            this.CreatedBy = createdBy;
            this.CreatedTime = createdTime;
            this.LastUpdatedTime = lastUpdatedTime;
            this.UnsubscribedTime = unsubscribedTime;
        }

        public string ApplicationName { get; set; }

        public string SubscriptionId { get; set; }

        public string CreatedBy { get; set; }

        public string Status { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? UnsubscribedTime { get; set; }
    }
}
