using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Routing.Clients
{
    public class ExecutionStatus
    {
        public const string SCHEDULED = "Scheduled";
        public const string RUNNING = "Running";
        public const string COMPLETED = "Completed";
        public const string FAILED = "Failed";
        public const string CANCELLED = "Cancelled";
        public const string UNKNOWN = "Unknown";

        public static string FromAzureMLPipelineRunStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case 0:
                    return SCHEDULED;
                case 1:
                    return RUNNING;
                case 2:
                    return COMPLETED;
                case 3:
                    return FAILED;
                case 4:
                    return CANCELLED;
                default:
                    return UNKNOWN;
            }
        }

        public static string FromAzureMLPipelineRunStatusDetail(string statusDetails)
        {
            switch (statusDetails)
            {
                case "Running":
                case "Queued":
                    return RUNNING;
                case "Failed":
                    return FAILED;
                case "Canceled":
                    return CANCELLED;
                case "Completed":
                    return COMPLETED;
                default:
                    return UNKNOWN;
            }
        }

        public static bool IsAMLPipelineRunCancellable(string statusDetails)
        {
            return statusDetails.Equals("Running", StringComparison.InvariantCultureIgnoreCase) || 
                statusDetails.Equals("Queued", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsAMLCompletedStatus(string statusDetails)
        {
            return statusDetails.Equals("Completed", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
