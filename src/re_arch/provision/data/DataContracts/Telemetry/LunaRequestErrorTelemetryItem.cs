using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Data
{
    public class LunaRequestErrorTelemetryItem: TableEntity
    {
        public LunaRequestErrorTelemetryItem(string subscriptionId)
        {
            this.PartitionKey = subscriptionId;
            this.RowKey = Guid.NewGuid().ToString();
        }

        public LunaRequestErrorTelemetryItem(
            string subscriptionId,
            string errorMessage,
            int httpStatusCode,
            DateTime requestStartTime,
            int? userErrorCode) :
            this(subscriptionId)
        {
            this.SubscriptionId = subscriptionId;
            this.ErrorMessage = errorMessage;
            this.HttpStatusCode = httpStatusCode;
            this.RequestStartTime = requestStartTime;
            this.userErrorCode = userErrorCode;
        }

        public string SubscriptionId { get; set; }

        public string ErrorMessage { get; set; }

        public int HttpStatusCode { get; set; }

        public DateTime RequestStartTime { get; set; }

        public int? userErrorCode { get; set; }
    }
}
