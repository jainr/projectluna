using Luna.Publish.Public.Client;
using System.Threading.Tasks;

namespace Luna.Publish.Clients
{
    public interface IHttpRequestParser
    {
        /// <summary>
        /// Parse and validate a Luna application from request body
        /// </summary>
        /// <param name="requestBody">The http request body</param>
        /// <returns>A LunaApplication instance</returns>
        Task<LunaApplicationProp> ParseAndValidateLunaApplicationAsync(string requestBody);

        /// <summary>
        /// Parse and validate a Luna API from request body
        /// </summary>
        /// <param name="requestBody">The http request body</param>
        /// <returns>A BaseLunaAPIProp instance</returns>
        Task<BaseLunaAPIProp> ParseAndValidateLunaAPIAsync(string requestBody);

        /// <summary>
        /// Parse and validate a Luna API version request body
        /// </summary>
        /// <param name="requestBody">The http request body</param>
        /// <param name="apiType">The API type</param>
        /// <returns>A BaseLunaAPIProp instance</returns>
        Task<BaseAPIVersionProp> ParseAndValidateAPIVersionAsync(string requestBody, string apiType);
    }
}
