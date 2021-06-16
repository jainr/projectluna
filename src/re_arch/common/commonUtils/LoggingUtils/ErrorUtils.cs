using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Luna.Common.Utils
{
    public class ErrorUtils
    {
        /// <summary>
        /// Check if the error code is retryable. Only retry on 500, 502, 503 and 504
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static bool IsHttpErrorCodeRetryable(HttpStatusCode errorCode)
        {
            return errorCode == HttpStatusCode.InternalServerError ||
                errorCode == HttpStatusCode.BadGateway ||
                errorCode == HttpStatusCode.ServiceUnavailable ||
                errorCode == HttpStatusCode.GatewayTimeout;

        }

        public static JsonResult HandleExceptions(Exception ex, ILogger logger, string traceId)
        {
            var errorModel = new ErrorModel(ex, traceId);
            logger.LogError(errorModel.ToString());
            return errorModel.GetHttpResult();
        }
    }
}
