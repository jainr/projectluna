using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Data
{
    public class LunaRequestTelemetryItem: TableEntity
    {
        public LunaRequestTelemetryItem(string subscriptionId, string timeWindow)
        {
            this.PartitionKey = subscriptionId;
            this.RowKey = timeWindow;
        }

        public LunaRequestTelemetryItem(
            string subscriptionId, 
            string timeWindow,
            int totalRequests,
            int failedRequests,
            int aveResponseTimeInMS) :
            this(subscriptionId, timeWindow)
        {
            this.SubscriptionId = subscriptionId;
            this.TimeWindow = timeWindow;
            this.TotalRequests = totalRequests;
            this.FailedRequests = failedRequests;
            this.AveResponseTimeInMS = aveResponseTimeInMS;
        }

        public string SubscriptionId { get; set; }

        public string TimeWindow { get; set; }

        public int TotalRequests { get; set; }

        public int FailedRequests { get; set; }

        public int AveResponseTimeInMS { get; set; }
    }
}
