using Luna.Common.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Luna.PubSub.Public.Client
{
    public class LunaQueueMessage
    {
        private const int EVENT_WAITING_TIMEOUT_IN_MS = 30 * 1000;
        private const int EVENT_WAITING_INTERVAL_IN_MS = 100;

        public string EventType { get; set; }

        public string PartitionKey { get; set; }

        public long EventSequenceId { get; set; }

        public void YieldTo(ConcurrentDictionary<string, long> inProcess, ILogger logger, int timeoutInMS = 0, int intervalInMS = 0)
        {
            timeoutInMS = timeoutInMS == 0 ? EVENT_WAITING_TIMEOUT_IN_MS : timeoutInMS;
            intervalInMS = intervalInMS == 0 ? EVENT_WAITING_INTERVAL_IN_MS : intervalInMS;

            int waitTimeInMS = 0;

            while (!inProcess.TryAdd(this.PartitionKey, this.EventSequenceId))
            {
                logger.LogDebug($"Yield to existing process for {this.PartitionKey} with id {this.EventSequenceId}. Total wait time {waitTimeInMS} ms.");

                var eventSequenceId = inProcess[this.PartitionKey];

                if (this.EventSequenceId < eventSequenceId)
                {
                    logger.LogInformation($"The event {eventSequenceId} is being processed, skipping event {this.EventSequenceId}.");
                    return;
                }

                if (waitTimeInMS >= timeoutInMS)
                {
                    var errorMessage = $"The event with partition key {this.PartitionKey} and id {this.EventSequenceId} " +
                        $"has been waiting for more than {timeoutInMS} seconds. Abort the processing.";

                    throw new LunaServerException(errorMessage);
                }

                waitTimeInMS += intervalInMS;
                Thread.Sleep(intervalInMS);
            }
        }

    }
}
