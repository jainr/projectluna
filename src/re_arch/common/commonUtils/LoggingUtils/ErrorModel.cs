using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Luna.Common.Utils.LoggingUtils
{
    public class ErrorModel
    {
        public ErrorModel(Exception ex, string traceId)
        {
            this.Message = ex.Message;
            this.TraceId = traceId;
            this.ErrorCode = UserErrorCode.InternalServerError;
            this.HttpStatusCode = HttpStatusCode.InternalServerError;

            if (ex is LunaException)
            {
                if (ex is LunaUserException)
                {
                    this.ErrorCode = ((LunaUserException)ex).ErrorCode;
                    this.HttpStatusCode = ((LunaUserException)ex).HttpStatusCode;
                }
            }
        }

        public string Message { get; set; }
        public string TraceId { get; set; }
        public UserErrorCode ErrorCode { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }

        public override string ToString()
        {
            return string.Format("Error message: {0} Error Code: {1}, Http Status Code: {2}, Trace Id: {3}",
                this.Message,
                this.ErrorCode.ToString(),
                this.HttpStatusCode.ToString(),
                this.TraceId);
        }

        public JsonResult GetHttpResult()
        {
            var result = new JsonResult(this);
            result.StatusCode = (int)this.HttpStatusCode;
            return result;
        }
    }
}
