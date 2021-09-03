﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Luna.Common.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luna.Publish.Public.Client
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
        public async Task<LunaApplicationMasterKeysResponse> RegenerateApplicationMasterKeys(
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

            return await GetResponseObject<LunaApplicationMasterKeysResponse>(response);
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
        public async Task<LunaApplicationMasterKeysResponse> GetApplicationMasterKeys(
            string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}/masterkeys");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<LunaApplicationMasterKeysResponse>(response);
        }

        /// <summary>
        /// Create Luna applciation
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The application properties</returns>
        public async Task<string> CreateLunaApplication(
            string name,
            string properties,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));
            ValidationUtils.ValidateInput<LunaApplicationRequest>(properties);

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, properties, headers);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Create Luna API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="properties">The properties</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The API properties</returns>
        public async Task<string> CreateLunaAPI(
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

            var content = await response.Content.ReadAsStringAsync();
            return content;
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
        public async Task<string> UpdateLunaApplication(
            string name,
            string properties,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));
            ValidationUtils.ValidateInput<LunaApplicationRequest>(properties);

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, properties, headers);

            var content = await response.Content.ReadAsStringAsync();
            return content;
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
        public async Task<string> GetLunaApplication(
            string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        /// Get a Luna API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The api name</param>
        /// <param name="headers">The luna request header</param>
        /// <returns>The Luna api properties</returns>
        public async Task<string> GetLunaAPI(
            string appName,
            string apiName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(appName, nameof(appName));
            ValidationUtils.ValidateObjectId(apiName, nameof(apiName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"applications/{appName}/apis/{apiName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        /// <summary>
        /// Create or update Azure marketplace offer from template
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="template">The offer template</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task<MarketplaceOffer> CreateOrUpdateMarketplaceOfferFromTemplateAsync(string name,
            string template,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}/template");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, template, headers);

            return await GetResponseObject<MarketplaceOffer>(response);
        }

        /// <summary>
        /// Create an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="offer">The offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task<MarketplaceOffer> CreateMarketplaceOfferAsync(string name,
            MarketplaceOffer offer,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}");

            var content = JsonConvert.SerializeObject(offer);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, content, headers);

            return await GetResponseObject<MarketplaceOffer>(response);
        }

        /// <summary>
        /// Update an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="offer">The offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task<MarketplaceOffer> UpdateMarketplaceOfferAsync(string name,
            MarketplaceOffer offer,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}");

            var content = JsonConvert.SerializeObject(offer);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, content, headers);

            return await GetResponseObject<MarketplaceOffer>(response);
        }

        /// <summary>
        /// Publish an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task PublishMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}/publish");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Post, uri, null, headers);

            return;
        }

        /// <summary>
        /// Delete an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer created</returns>
        public async Task DeleteMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }

        /// Create a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="plan">The plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        public async Task<MarketplacePlan> CreateMarketplacePlanAsync(string offerName,
            string planName,
            MarketplacePlan plan,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName, 
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, 
                nameof(offerName));

            ValidationUtils.ValidateStringValueLength(planName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(planName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans/{planName}");

            var content = JsonConvert.SerializeObject(plan);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, content, headers);

            return await GetResponseObject<MarketplacePlan>(response);
        }

        /// Update a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="plan">The plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        public async Task<MarketplacePlan> UpdateMarketplacePlanAsync(string offerName,
            string planName,
            MarketplacePlan plan,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(offerName));

            ValidationUtils.ValidateStringValueLength(planName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(planName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans/{planName}");

            var content = JsonConvert.SerializeObject(plan);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, content, headers);

            return await GetResponseObject<MarketplacePlan>(response);
        }

        /// Delete a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan created</returns>
        public async Task DeleteMarketplacePlanAsync(string offerName,
            string planName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(offerName));

            ValidationUtils.ValidateStringValueLength(planName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(planName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans/{planName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }

        /// <summary>
        /// Get an Azure marketplace offer
        /// </summary>
        /// <param name="name">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        public async Task<JObject> GetMarketplaceOfferAsync(string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(name, ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<JObject>(response);
        }

        /// <summary>
        /// List Azure marketplace offers
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The offer</returns>
        public async Task<List<MarketplaceOffer>> ListMarketplaceOffersAsync(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<List<MarketplaceOffer>>(response);
        }

        /// Get a plan in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="planName">Name of the plan</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plan</returns>
        public async Task<MarketplacePlan> GetMarketplacePlanAsync(string offerName,
            string planName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(offerName));

            ValidationUtils.ValidateStringValueLength(planName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(planName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans/{planName}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<MarketplacePlan>(response);
        }

        /// List plans in Azure marketplace offer
        /// </summary>
        /// <param name="offerName">Name of the offer</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The plans</returns>
        public async Task<List<MarketplacePlan>> ListMarketplacePlansAsync(string offerName,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateStringValueLength(offerName,
                ValidationUtils.AZURE_MARKETPLACE_OBJECT_STRING_MAX_LENGTH,
                nameof(offerName));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"marketplace/offers/{offerName}/plans");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<List<MarketplacePlan>>(response);
        }

        /// <summary>
        /// Create an automation webhook
        /// </summary>
        /// <param name="name">Name of the automation webhook</param>
        /// <param name="webhook">The webhook</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The webhook created</returns>
        public async Task<AutomationWebhook> CreateAutomationWebhookAsync(string name,
            AutomationWebhook webhook,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"reviewsettings/webhooks/{name}");

            var content = JsonConvert.SerializeObject(webhook);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Put, uri, content, headers);

            return await GetResponseObject<AutomationWebhook>(response);
        }

        /// <summary>
        /// Update an automation webhook
        /// </summary>
        /// <param name="name">Name of the automation webhook</param>
        /// <param name="webhook">The webhook</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The webhook updated</returns>
        public async Task<AutomationWebhook> UpdateAutomationWebhookAsync(string name,
            AutomationWebhook webhook,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"reviewsettings/webhooks/{name}");

            var content = JsonConvert.SerializeObject(webhook);

            var response = await SendRequestAndVerifySuccess(HttpMethod.Patch, uri, content, headers);

            return await GetResponseObject<AutomationWebhook>(response);
        }

        /// <summary>
        /// Get an automation webhook
        /// </summary>
        /// <param name="name">Name of the automation webhook</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The webhook</returns>
        public async Task<AutomationWebhook> GetAutomationWebhookAsync(string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"reviewsettings/webhooks/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<AutomationWebhook>(response);
        }

        /// <summary>
        /// List automation webhooks
        /// </summary>
        /// <param name="headers">The Luna request headers</param>
        /// <returns>The webhooks</returns>
        public async Task<List<AutomationWebhook>> ListAutomationWebhooksAsync(LunaRequestHeaders headers)
        {
            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"reviewsettings/webhooks");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Get, uri, null, headers);

            return await GetResponseObject<List<AutomationWebhook>>(response);
        }

        /// <summary>
        /// Delete an automation webhook
        /// </summary>
        /// <param name="name">Name of the automation webhook</param>
        /// <param name="headers">The Luna request headers</param>
        /// <returns></returns>
        public async Task DeleteAutomationWebhookAsync(string name,
            LunaRequestHeaders headers)
        {
            ValidationUtils.ValidateObjectId(name, nameof(name));

            headers.AzureFunctionKey = this._config.AuthenticationKey;
            var uri = new Uri(this._config.ServiceBaseUrl + $"reviewsettings/webhooks/{name}");

            var response = await SendRequestAndVerifySuccess(HttpMethod.Delete, uri, null, headers);

            return;
        }

        private async Task<T> GetResponseObject<T>(HttpResponseMessage response, TypeNameHandling typeNameHandling = TypeNameHandling.Auto)
        {
            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings()
            {
                TypeNameHandling = typeNameHandling
            });

            return obj;
        }
    }
}
