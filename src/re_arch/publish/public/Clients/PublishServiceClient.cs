using Luna.Common.Utils;
using Luna.Common.Utils.RestClients;
using Luna.Publish.Public.Client.DataContract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Publish.PublicClient.Clients
{
    public class PublishServiceClient : RestClient, IPublishServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PublishServiceClient> _logger;
        private readonly PublishServiceClientConfiguration _config;

        [ActivatorUtilitiesConstructor]
        public PublishServiceClient(IOptionsMonitor<PublishServiceClientConfiguration> option,
            HttpClient httpClient,
            ILogger<PublishServiceClient> logger) :
            base(option, httpClient, logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._config = option.CurrentValue ?? throw new ArgumentNullException(nameof(option.CurrentValue));
        }

        /// <summary>
        /// Regenerate application master keys
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="keyName">The key name</param>
        /// <param name="headers">the luna request header</param>
        /// <returns></returns>
        public async Task<LunaApplicationMasterKeys> RegenerateApplicationMasterKeys(
            string appName, 
            string keyName, 
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateStringInList(keyName, PublishQueryParameterConstants.GetValidKeyNames(), nameof(keyName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + 
                $"applications/{appName}/regenerateMasterKeys?{PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME}={keyName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, null, headers);

            return await GetResponseObject<LunaApplicationMasterKeys>(response);
        }

        /// <summary>
        /// List Luna applications owned by the caller
        /// </summary>
        /// <param name="isAdmin">If current user is admin</param>
        /// <param name="headers">The luna request header containing user id</param>
        /// <returns>All Luna applications owned by the caller</returns>
        public async Task<List<LunaApplication>> ListLunaApplications(
            bool isAdmin, 
            LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications");

            if (isAdmin)
            {
                uri = new Uri(this._config.ServiceBaseUrl + 
                    $"applications?{PublishQueryParameterConstants.ROLE_QUERY_PARAMETER_NAME}={PublishQueryParameterConstants.ADMIN_ROLE_NAME}");
            }

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<List<LunaApplication>>(response);
        }

        /// <summary>
        /// Get Luna applciation master keys
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The application master keys</returns>
        public async Task<LunaApplicationMasterKeys> GetApplicationMasterKeys(
            string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}/masterkeys");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<LunaApplicationMasterKeys>(response);
        }

        /// <summary>
        /// Create Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The application properties</returns>
        public async Task<LunaApplicationProp> CreateLunaApplication(
            string name,
            string properties,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));
            ValidationUtils.ValidateInput<LunaApplicationProp>(properties);

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, properties, headers);

            return await GetResponseObject<LunaApplicationProp>(response);
        }

        /// <summary>
        /// Create Luna API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API properties</returns>
        public async Task<BaseLunaAPIProp> CreateLunaAPI(
            string appName,
            string apiName,
            string properties,
            LunaRequestHeaders headers)
        {

            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(apiName, nameof(apiName));
            ValidationUtils.ValidateInput<BaseLunaAPIProp>(properties);

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{appName}/apis/{apiName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, properties, headers);

            return await GetResponseObject<BaseLunaAPIProp>(response);
        }

        /// <summary>
        /// Create Luna API version
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="versionName">The version name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API Version properties</returns>
        public async Task<BaseAPIVersionProp> CreateLunaAPIVersion(
            string appName,
            string apiName,
            string versionName,
            string properties,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(apiName, nameof(apiName));
            ValidationUtils.ValidateObjectId(versionName, nameof(versionName));
            ValidationUtils.ValidateInput<BaseAPIVersionProp>(properties);

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{appName}/apis/{apiName}/versions/{versionName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, properties, headers);

            return await GetResponseObject<BaseAPIVersionProp>(response);
        }


        /// <summary>
        /// Update Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The application properties</returns>
        public async Task<LunaApplicationProp> UpdateLunaApplication(
            string name,
            string properties,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));
            ValidationUtils.ValidateInput<LunaApplicationProp>(properties);

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, properties, headers);

            return await GetResponseObject<LunaApplicationProp>(response);
        }

        /// <summary>
        /// Update Luna API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API properties</returns>
        public async Task<BaseLunaAPIProp> UpdateLunaAPI(
            string appName,
            string apiName,
            string properties,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(apiName, nameof(apiName));
            ValidationUtils.ValidateInput<BaseLunaAPIProp>(properties);

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{appName}/apis/{apiName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, properties, headers);

            return await GetResponseObject<BaseLunaAPIProp>(response);
        }

        /// <summary>
        /// Update Luna API version
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="versionName">The version name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API version properties</returns>
        public async Task<BaseAPIVersionProp> UpdateLunaAPIVersion(
            string appName,
            string apiName,
            string versionName,
            string properties,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(apiName, nameof(apiName));
            ValidationUtils.ValidateObjectId(apiName, nameof(versionName));
            ValidationUtils.ValidateInput<BaseAPIVersionProp>(properties);

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{appName}/apis/{apiName}/versions/{versionName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, properties, headers);

            return await GetResponseObject<BaseAPIVersionProp>(response);
        }

        /// <summary>
        /// Delete Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>True if deleted, false otherwise</returns>
        public async Task<bool> DeleteLunaApplication(
            string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete Luna API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>True if deleted, false otherwise</returns>
        public async Task<bool> DeleteLunaAPI(
            string appName,
            string apiName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(apiName, nameof(apiName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{appName}/apis/{apiName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete Luna API version
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="versionName">The version name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>True if deleted, false otherwise</returns>
        public async Task<bool> DeleteLunaAPIVersion(
            string appName,
            string apiName,
            string versionName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(apiName, nameof(apiName));
            ValidationUtils.ValidateObjectId(versionName, nameof(versionName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{appName}/apis/{apiName}/versions/{versionName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Publish Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="comments">The publish comments</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>True if the application is published, false otherwise</returns>
        public async Task<bool> PublishLunaApplication(
            string name,
            string comments,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));
            ValidationUtils.ValidateStringValueLength(comments, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(comments));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}/publish");

            if (!string.IsNullOrEmpty(comments))
            {
                uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}/publish?comments={comments}");
            }

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, null, headers);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get a Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The Luna application properties</returns>
        public async Task<LunaApplication> GetLunaApplication(
            string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<LunaApplication>(response);
        }

        private async Task<T> GetResponseObject<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            return obj;
        }

    }
}
