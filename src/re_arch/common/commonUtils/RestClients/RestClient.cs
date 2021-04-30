using Luna.Common.Utils.LoggingUtils;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Luna.Common.Utils.RestClients
{
    public class RestClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RestClient> _logger;
        private readonly RestClientConfiguration _config;

        public RestClient(IOptionsMonitor<RestClientConfiguration> option,
            HttpClient httpClient,
            ILogger<RestClient> logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._config = option.CurrentValue ?? throw new ArgumentNullException(nameof(option.CurrentValue));
        }

        /// <summary>
        /// Build the http request message
        /// </summary>
        /// <param name="method">The http method</param>
        /// <param name="requestUri">The request uri</param>
        /// <param name="content">The request content</param>
        /// <param name="headers">The Luna service headers</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> SendRequestAndVerifySuccess(
            HttpMethod method,
            Uri requestUri,
            string content,
            LunaRequestHeaders headers)
        {
            var response = await SendRequest(method, requestUri, content, headers);
            
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var errorModel = JsonConvert.DeserializeObject<ErrorModel>(responseContent);
                if (errorModel != null)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            throw new LunaBadRequestUserException(errorModel.Message, errorModel.ErrorCode);
                        case HttpStatusCode.NotFound:
                            throw new LunaNotFoundUserException(errorModel.Message);
                        case HttpStatusCode.Unauthorized:
                            throw new LunaUnauthorizedUserException(errorModel.Message);
                        case HttpStatusCode.Conflict:
                            throw new LunaConflictUserException(errorModel.Message);
                        case HttpStatusCode.NotImplemented:
                            throw new LunaNotSupportedUserException(errorModel.Message);
                        default:
                            break;
                    }
                }

                throw new LunaServerException(
                    $"Http request failed with response code {response.StatusCode} and error {responseContent}.");
            }
        }

        /// <summary>
        /// Build the http request message
        /// </summary>
        /// <param name="method">The http method</param>
        /// <param name="requestUri">The request uri</param>
        /// <param name="content">The request content</param>
        /// <param name="headers">The Luna service headers</param>
        /// <returns></returns>
        private static HttpRequestMessage BuildRequest(
        HttpMethod method,
        Uri requestUri,
        string content,
        LunaRequestHeaders headers)
        {
            var request = new HttpRequestMessage { RequestUri = requestUri, Method = method };
            headers.AddToHttpRequestHeaders(request.Headers);

            if (content != null && (method.Equals(HttpMethod.Post) ||
                method.Equals(HttpMethod.Put) || 
                method.Equals(HttpMethod.Patch)))
            {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return request;
        }

        /// <summary>
        /// Send a simple request authenticated with a bearer token
        /// </summary>
        /// <param name="method">The http method</param>
        /// <param name="requestUri">The request uri</param>
        /// <param name="content">The request content</param>
        /// <param name="headers">The Luna service headers</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> SendRequest(
            HttpMethod method,
            Uri requestUri,
            string content,
            LunaRequestHeaders headers)
        {
            var request = BuildRequest(
                method,
                requestUri,
                content,
                headers);

            return await _httpClient.SendAsync(request);
        }
    }
}
