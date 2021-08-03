using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Luna.Common.Utils
{
    public class ErrorModel
    {
        public ErrorModel()
        {

        }

        public ErrorModel(Exception ex, string traceId)
        {
            this.Message = ex.Message;
            this.TraceId = traceId;
            this.ErrorCode = UserErrorCode.InternalServerError;
            this.HttpStatusCode = HttpStatusCode.InternalServerError;
            this.StackTrace = ex.StackTrace;

            if (ex is LunaException)
            {
                if (ex is LunaUserException)
                {
                    this.ErrorCode = ((LunaUserException)ex).ErrorCode;
                    this.HttpStatusCode = ((LunaUserException)ex).HttpStatusCode;
                }
            }
            else
            {
                if (ex.InnerException != null && ex.InnerException is LunaUserException)
                {
                    this.Message = ex.InnerException.Message;
                    this.ErrorCode = ((LunaUserException)ex.InnerException).ErrorCode;
                    this.HttpStatusCode = ((LunaUserException)ex.InnerException).HttpStatusCode;
                }
            }  
        }

        public string Message { get; set; }
        public string TraceId { get; set; }
        public UserErrorCode ErrorCode { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }

        [JsonIgnore]
        public string StackTrace { get; set; }

        public override string ToString()
        {
            return string.Format("Error message: {0} Error Code: {1}, Http Status Code: {2}, Trace Id: {3}, Stack trace {4}",
                this.Message,
                this.ErrorCode.ToString(),
                this.HttpStatusCode.ToString(),
                this.TraceId,
                this.StackTrace);
        }

        public ContentResult GetHttpResult()
        {
            ErrorModel content = null;
            if (this.HttpStatusCode == HttpStatusCode.InternalServerError)
            {
                content = new ErrorModel()
                {
                    Message = ErrorMessages.INTERNAL_SERVER_ERROR,
                    TraceId = this.TraceId,
                    ErrorCode = this.ErrorCode,
                    HttpStatusCode = this.HttpStatusCode
                };
            }
            else
            {
                content = this;
            }

            ContentResult result = new ContentResult()
            {
                Content = JsonConvert.SerializeObject(content),
                ContentType = "application/json",
                StatusCode = (int)this.HttpStatusCode
            };

            result.StatusCode = (int)this.HttpStatusCode;
            return result;
        }
    }
}
