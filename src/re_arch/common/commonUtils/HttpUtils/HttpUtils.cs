using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Luna.Common.Utils
{
    public class HttpUtils
    {
        public static readonly string JSON_CONTENT_TYPE = "application/json";

        public static async Task<T> DeserializeRequestBodyAsync<T>(HttpRequest req)
        {
            string requestBody = await GetRequestBodyAsync(req);
            T obj = (T)JsonConvert.DeserializeObject(requestBody, typeof(T), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            return obj;
        }

        public static async Task<string> GetRequestBodyAsync(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            return requestBody;
        }

        public static LunaRequestHeaders GetLunaRequestHeaders(HttpRequest req)
        {
            return new LunaRequestHeaders(req);
        }
    }
}
