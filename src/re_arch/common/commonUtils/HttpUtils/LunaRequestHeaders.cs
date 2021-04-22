using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.HttpUtils
{
    public class LunaRequestHeaders
    {
        public LunaRequestHeaders(HttpRequest req)
        {
            this.Caller = req.Headers.ContainsKey("Luna-Caller") ? req.Headers["Luna-Caller"].ToString() : string.Empty;
            this.TraceId = req.Headers.ContainsKey("Luna-Trace-Id") ? req.Headers["Luna-Trace-Id"].ToString() : string.Empty;
        }

        public string Caller { get; }

        public string TraceId { get; }
    }
}
