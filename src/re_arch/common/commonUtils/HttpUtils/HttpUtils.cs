using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Luna.Common.Utils.HttpUtils
{
    public class HttpUtils
    {
        public static async Task<T> DeserializeRequestBody<T>(HttpRequest req)
        {
            string requestBody = await GetRequestBody(req);
            T obj = (T)JsonConvert.DeserializeObject(requestBody, typeof(T), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            return obj;
        }

        public static async Task<string> GetRequestBody(HttpRequest req)
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
