using Luna.Common.Utils.LoggingUtils.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Luna.Common.Utils.LoggingUtils.Exceptions
{
    public class LunaUserException: LunaException
    {
        public UserErrorCode ErrorCode { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }

        public string Target { get; set; }

        public LunaUserException(string message, 
            UserErrorCode code, 
            HttpStatusCode statusCode, 
            string target = "method_error") : base(message)
        {
            this.ErrorCode = code;
            this.HttpStatusCode = statusCode;
            this.Target = target;
        }
    }
}
