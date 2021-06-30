using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils
{
    public static class LoggingExtensions
    {

        public static IDisposable BeginQueueTriggerNamedScope(this ILogger logger, CloudQueueMessage queueMessage)
        {
            var dict = new Dictionary<string, object>();
            dict.Add("Luna.QueueMessageId", queueMessage.Id);
            dict.Add("Luna.QueueMessageDequeueCount", queueMessage.DequeueCount);
            dict.Add("Luna.QueueMessageInsertionTime", queueMessage.InsertionTime);
            return logger.BeginScope(dict);
        }

        public static IDisposable BeginManagementNamedScope(this ILogger logger,
            LunaRequestHeaders header)
        {
            var dictionary = header.GetManagementLoggingScopeProperties();
            return logger.BeginScope(dictionary);
        }

        public static IDisposable BeginRoutingNamedScope(this ILogger logger,
            string appName,
            string apiName,
            string apiVersion,
            string operationId,
            LunaRequestHeaders header,
            string operationName = null)
        {
            var dictionary = header.GetRoutingLoggingScopeProperties();
            dictionary.Add("Luna.AppName", appName);
            dictionary.Add("Luna.APIName", apiName);
            dictionary.Add("Luna.APIVersion", apiVersion);
            dictionary.Add("Luna.OperationId", operationId);
            dictionary.Add("Luna.OperationName", operationName);
            return logger.BeginScope(dictionary);
        }

        public static void LogMethodBegin(this ILogger logger, string methodName)
        {
            logger.LogInformation($"[FxBegin][{methodName}] Function {methodName} begins.");
        }
        public static void LogMethodEnd(this ILogger logger, string methodName, long elapsedTimeInMS = -1)
        {
            if (elapsedTimeInMS == -1)
            {
                logger.LogInformation($"[FxEnd][{methodName}] Function {methodName} ends.");
            }
            else
            {
                logger.LogInformation($"[FxEnd][{methodName}] Function {methodName} ends in {elapsedTimeInMS} ms.");
            }
        }

        public static void LogRoutingRequestBegin(this ILogger logger, string methodName)
        {
            logger.LogInformation($"[FxBegin][{methodName}] Request {methodName} begins.");
        }

        public static void LogRoutingRequestEnd(this ILogger logger, 
            string methodName, 
            int? statusCode, 
            string subscriptionId,
            long elapsedTimeInMS)
        {
            var dict = new Dictionary<string, object>();
            dict.Add("Luna.SubscriptionId", subscriptionId);
            dict.Add("Luna.HttpStatusCode", statusCode ?? -1);
            dict.Add("Luna.ElapsedTimeInMS", elapsedTimeInMS);
            using (logger.BeginScope(dict))
            {
                logger.LogInformation($"[FxEnd][{methodName}] Request {methodName} ends with HttpStatusCode {statusCode ?? -1} in {elapsedTimeInMS} ms.");
            }
        }
    }
}
