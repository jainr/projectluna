using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Luna.Common.Utils
{
    public class LunaRequestHeaders
    {
        private const string LUNA_USER_ID_HEADER_NAME = "Luna-User-Id";
        private const string LUNA_USER_NAME_HEADER_NAME = "Luna-User-Name";
        private const string LUNA_SUBSCRIPTION_ID_HEADER_NAME = "Luna-Subscription-Id";
        private const string LUNA_TRACE_ID_HEADER_NAME = "Luna-Trace-Id";
        private const string LUNA_APPLICATION_MASTER_KEY = "Luna-Application-Master-Key";
        private const string LUNA_SUBCRIPTION_KEY = "api-key";
        private const string AAD_USER_ID = "X-MS-CLIENT-PRINCIPAL-ID";
        private const string AAD_USER_NAME = "X-MS-CLIENT-PRINCIPAL-NAME";

        private const string AZURE_FUNCTION_KEY = "x-functions-key";

        public LunaRequestHeaders()
        {
        }

        public LunaRequestHeaders(HttpRequest req)
        {
            this.LunaSubscriptionKey = req.Headers.ContainsKey(LUNA_SUBCRIPTION_KEY) ?
                req.Headers[LUNA_SUBCRIPTION_KEY].ToString() : string.Empty;

            this.UserId = req.Headers.ContainsKey(LUNA_USER_ID_HEADER_NAME) ? 
                req.Headers[LUNA_USER_ID_HEADER_NAME].ToString() : string.Empty;

            // Overwrite the user id if AAD auth is used
            this.UserId = req.Headers.ContainsKey(AAD_USER_ID) ?
                req.Headers[AAD_USER_ID].ToString() : this.UserId;

            this.UserName = req.Headers.ContainsKey(LUNA_USER_NAME_HEADER_NAME) ?
                req.Headers[LUNA_USER_NAME_HEADER_NAME].ToString() : string.Empty;

            // Overwrite the user name if AAD auth is used
            this.UserName = req.Headers.ContainsKey(AAD_USER_NAME) ?
                req.Headers[AAD_USER_NAME].ToString() : this.UserName;

            // Generate a new TraceId if not included in the header
            this.TraceId = req.Headers.ContainsKey(LUNA_TRACE_ID_HEADER_NAME) ? 
                req.Headers[LUNA_TRACE_ID_HEADER_NAME].ToString() : Guid.NewGuid().ToString();

            this.SubscriptionId = req.Headers.ContainsKey(LUNA_SUBSCRIPTION_ID_HEADER_NAME) ? 
                req.Headers[LUNA_SUBSCRIPTION_ID_HEADER_NAME].ToString() : string.Empty;

            this.LunaApplicationMasterKey = req.Headers.ContainsKey(LUNA_APPLICATION_MASTER_KEY) ?
                req.Headers[LUNA_APPLICATION_MASTER_KEY].ToString() : string.Empty;

            // Do not phase the function keys! They are validated by Azure functions.
        }

        public string LunaSubscriptionKey { get; set; }

        public string SubscriptionId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string TraceId { get; set; }

        public string AzureFunctionKey { get; set; }

        public string LunaApplicationMasterKey { get; set; }

        public LunaRequestHeaders GetPassThroughHeaders()
        {
            return new LunaRequestHeaders()
            {
                SubscriptionId = this.SubscriptionId,
                UserId = this.UserId,
                UserName = this.UserName,
                TraceId = this.TraceId
            };
        }

        public Dictionary<string, object> GetManagementLoggingScopeProperties()
        {
            var properties = new Dictionary<string, object>();
            properties.Add("Luna.UserName", this.UserName);
            properties.Add("Luna.UserId", this.UserId);
            properties.Add("Luna.TraceId", this.TraceId);
            return properties;
        }

        public Dictionary<string, object> GetRoutingLoggingScopeProperties()
        {
            var properties = new Dictionary<string, object>();
            properties.Add("Luna.SubscriptionId", this.SubscriptionId);
            properties.Add("Luna.UserId", this.UserId);
            properties.Add("Luna.TraceId", this.TraceId);
            return properties;
        }

        public void AddToHttpRequestHeaders(HttpRequestHeaders headers)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                headers.Add(LUNA_USER_ID_HEADER_NAME, this.UserId);
            }

            if (!string.IsNullOrEmpty(UserName))
            {
                headers.Add(LUNA_USER_NAME_HEADER_NAME, this.UserName);
            }

            if (!string.IsNullOrEmpty(SubscriptionId))
            {
                headers.Add(LUNA_SUBSCRIPTION_ID_HEADER_NAME, this.SubscriptionId);
            }

            if (!string.IsNullOrEmpty(TraceId))
            {
                headers.Add(LUNA_TRACE_ID_HEADER_NAME, this.TraceId);
            }

            if (!string.IsNullOrEmpty(AzureFunctionKey))
            {
                headers.Add(AZURE_FUNCTION_KEY, this.AzureFunctionKey);
            }

            if (!string.IsNullOrEmpty(LunaApplicationMasterKey))
            {
                headers.Add(LUNA_APPLICATION_MASTER_KEY, this.LunaApplicationMasterKey);
            }
        }
    }
}
