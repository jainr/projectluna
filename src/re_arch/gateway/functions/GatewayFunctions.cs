using Luna.Common.Utils;
using Luna.Gallery.Public.Client;
using Luna.Publish.Public.Client;
using Luna.PubSub.Public.Client;
using Luna.Partner.Public.Client;
using Luna.RBAC.Public.Client;
using Luna.Marketplace.Public.Client;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Luna.Gateway.Functions
{
    public class GatewayFunctions
    {
        private readonly ILogger<GatewayFunctions> _logger;
        private readonly IGalleryServiceClient _galleryServiceClient;
        private readonly IMarketplaceServiceClient _marketplaceServiceClient;
        private readonly IRBACClient _rbacClient;

        public GatewayFunctions(IGalleryServiceClient galleryServiceClient,
            IMarketplaceServiceClient marketplaceClient,
            IRBACClient rbacClient,
            ILogger<GatewayFunctions> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._galleryServiceClient = galleryServiceClient ?? throw new ArgumentNullException(nameof(galleryServiceClient));
            this._marketplaceServiceClient = marketplaceClient ?? throw new ArgumentNullException(nameof(marketplaceClient));
            this._rbacClient = rbacClient ?? throw new ArgumentNullException(nameof(rbacClient));
        }

        [FunctionName("test")]
        public async Task<IActionResult> Test(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/test")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.Test));

                try
                {

                    HttpClient client = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/common/oauth2/devicecode?api-version-1.0");

                    request.Content = new StringContent("client_id=04b07795-8ddb-461a-bbee-02f9e1bf7b46&resource=https%3A%2F%2Fmanagement.core.windows.net%2F");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return new OkObjectResult(JObject.Parse(content));
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.Test));
                }
            }
        }

        #region Azure Marketplace

        /// <summary>
        /// Resolve Azure Marketplace subscription from token
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/marketplace/subscriptions/resolveToken</url>
        /// <param name="req" in="body"><see cref="string"/>Token</param>
        /// <response code="200">
        ///     <see cref="MarketplaceSubscriptionResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscriptionResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a marketplace subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ResolveMarketplaceSubscription")]
        public async Task<IActionResult> ResolveMarketplaceSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "marketplace/subscriptions/resolvetoken")]
            HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ResolveMarketplaceSubscription));

                try
                {
                    var token = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _marketplaceServiceClient.ResolveMarketplaceTokenAsync(token, lunaHeaders);

                    return new ContentResult
                    {
                        Content = result,
                        ContentType = HttpUtils.JSON_CONTENT_TYPE,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ResolveMarketplaceSubscription));
                }
            }
        }

        /// <summary>
        /// Create Azure Marketplace subscription
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/marketplace/subscriptions/{subscriptionId}</url>
        /// <param name="subscriptionId" required="true" cref="string" in="path">ID of the subscription</param>
        /// <param name="req" in="body">
        ///     <see cref="MarketplaceSubscriptionRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscriptionRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a marketplace subscription
        ///         </summary>
        ///     </example>
        ///     The subscription
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplaceSubscriptionResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscriptionResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a marketplace subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateMarketplaceSubscription")]
        public async Task<IActionResult> CreateMarketplaceSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Put", Route = "marketplace/subscriptions/{subscriptionId}")]
            HttpRequest req,
            Guid subscriptionId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateMarketplaceSubscription));

                try
                {
                    var content = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _marketplaceServiceClient.CreateMarketplaceSubscriptionAsync(subscriptionId, content, lunaHeaders);

                    return new ContentResult
                    {
                        Content = result,
                        ContentType = HttpUtils.JSON_CONTENT_TYPE,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateMarketplaceSubscription));
                }
            }
        }

        /// <summary>
        /// Get Azure Marketplace subscription
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/marketplace/subscriptions/{subscriptionId}</url>
        /// <param name="subscriptionId" required="true" cref="string" in="path">ID of the subscription</param>
        /// <param name="req">req</param>
        /// <response code="200">
        ///     <see cref="MarketplaceSubscriptionResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscriptionResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a marketplace subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetMarketplaceSubscription")]
        public async Task<IActionResult> GetMarketplaceSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "marketplace/subscriptions/{subscriptionId}")]
            HttpRequest req,
            Guid subscriptionId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMarketplaceSubscription));

                try
                {
                    var result = await _marketplaceServiceClient.GetMarketplaceSubscriptionAsync(subscriptionId, lunaHeaders);

                    return new ContentResult
                    {
                        Content = result,
                        ContentType = HttpUtils.JSON_CONTENT_TYPE,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetMarketplaceSubscription));
                }
            }
        }

        /// <summary>
        /// Delete Azure Marketplace subscription
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/marketplace/subscriptions/{subscriptionId}</url>
        /// <param name="subscriptionId" required="true" cref="string" in="path">ID of the subscription</param>
        /// <param name="req">req</param>
        /// <response code="200">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteMarketplaceSubscription")]
        public async Task<IActionResult> DeleteMarketplaceSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Delete", Route = "marketplace/subscriptions/{subscriptionId}")]
            HttpRequest req,
            Guid subscriptionId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteMarketplaceSubscription));

                try
                {
                    await _marketplaceServiceClient.UnsubscribeMarketplaceSubscriptionAsync(subscriptionId, lunaHeaders);

                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteMarketplaceSubscription));
                }
            }
        }

        /// <summary>
        /// List Azure Marketplace subscriptions
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/marketplace/subscriptions</url>
        /// <param name="req">req</param>
        /// <response code="200">
        /// <see cref="List{T}"/>
        ///     where T is <see cref="MarketplaceSubscriptionResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscriptionResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a marketplace subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListMarketplaceSubscriptions")]
        public async Task<IActionResult> ListMarketplaceSubscriptions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "marketplace/subscriptions")]
            HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMarketplaceSubscriptions));

                try
                {
                    var result = await _marketplaceServiceClient.ListMarketplaceSubscriptionsAsync(lunaHeaders);

                    return new ContentResult
                    {
                        Content = result,
                        ContentType = HttpUtils.JSON_CONTENT_TYPE,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListMarketplaceSubscriptions));
                }
            }
        }


        /// <summary>
        /// List Azure Marketplace subscription details
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/marketplace/subscriptiondetails</url>
        /// <param name="req">req</param>
        /// <response code="200">
        /// <see cref="List{T}"/>
        ///     where T is <see cref="MarketplaceSubscriptionResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscriptionResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a marketplace subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListMarketplaceSubscriptionDetails")]
        public async Task<IActionResult> ListMarketplaceSubscriptionDetails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "marketplace/subscriptiondetails")]
            HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMarketplaceSubscriptionDetails));

                try
                {
                    var result = await _marketplaceServiceClient.ListMarketplaceSubscriptionDetailsAsync(lunaHeaders);

                    return new ContentResult
                    {
                        Content = result,
                        ContentType = HttpUtils.JSON_CONTENT_TYPE,
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListMarketplaceSubscriptionDetails));
                }
            }
        }

        /// <summary>
        /// Get offer parameters
        /// </summary>
        /// <group>Gallery</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/plans/{planId}/parameters</url>
        /// <param name="offerId" required="true" cref="string" in="path">The offer ID</param>
        /// <param name="planId" required="true" cref="string" in="path">The plan ID</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="MarketplaceParameter"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceParameter.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a marketplace subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetMarketplaceParameters")]
        public async Task<IActionResult> GetMarketplaceParameters(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "marketplace/offers/{offerId}/plans/{planId}/parameters")]
            HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMarketplaceParameters));

                try
                {
                    var result = await _marketplaceServiceClient.GetMarketplaceParametersAsync(offerId, planId, lunaHeaders);
                    return new OkObjectResult(result);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetMarketplaceParameters));
                }
            }
        }
        #endregion

        #region gallery


        /// <summary>
        /// Get a published Luna application
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}</url>
        /// <param name="appName" required="true" cref="string" in="path">The name of the application</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="PublishedLunaApplication"/>
        ///     <example>
        ///         <value>
        ///             <see cref="PublishedLunaApplication.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a published Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetPublishedApplication")]
        public async Task<IActionResult> GetPublishedApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}")] HttpRequest req,
            string appName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetPublishedApplication));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"publishedApplications/{appName}", null, lunaHeaders))
                    {
                        var app = await _galleryServiceClient.GetLunaApplication(appName, lunaHeaders);
                        return new OkObjectResult(app);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPublishedApplication));
                }
            }
        }

        /// <summary>
        /// List published Luna applications
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/gallery/applications</url>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="PublishedLunaApplication"/>
        ///     <example>
        ///         <value>
        ///             <see cref="PublishedLunaApplication.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a published Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListPublishedApplications")]
        public async Task<IActionResult> ListPublishedApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListPublishedApplications));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"publishedApplications", null, lunaHeaders))
                    {
                        var appList = await _galleryServiceClient.ListLunaApplications(lunaHeaders);
                        return new OkObjectResult(appList);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListPublishedApplications));
                }
            }
        }

        /// <summary>
        /// Get swagger of a published Luna application
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/swagger</url>
        /// <param name="appName" required="true" cref="string" in="path">The name of the application</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSwagger"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSwagger.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of swagger of Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetPublishedApplicationSwagger")]
        public async Task<IActionResult> GetPublishedApplicationSwagger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}/swagger")] HttpRequest req,
            string appName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetPublishedApplicationSwagger));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"publishedApplications/{appName}", null, lunaHeaders))
                    {
                        var swagger = await _galleryServiceClient.GetLunaApplicationSwagger(appName, lunaHeaders);
                        return new OkObjectResult(swagger);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPublishedApplicationSwagger));
                }
            }
        }

        /// <summary>
        /// Get recommended applications for the specified Luna application
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/recommended</url>
        /// <param name="appName" required="true" cref="string" in="path">The name of the application</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="PublishedLunaApplication"/>
        ///     <example>
        ///         <value>
        ///             <see cref="PublishedLunaApplication.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a published Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetRecommendedPublishedApplications")]
        public async Task<IActionResult> GetRecommendedPublishedApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}/recommended")] HttpRequest req,
            string appName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetRecommendedPublishedApplications));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"publishedApplications/{appName}", null, lunaHeaders))
                    {
                        var appList = await _galleryServiceClient.GetRecommendedLunaApplications(appName, lunaHeaders);
                        return new OkObjectResult(appList);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetRecommendedPublishedApplications));
                }
            }
        }

        /// <summary>
        /// Create a subscription of a published Luna application
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/subscriptions/{subscriptionName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the Luna application</param>
        /// <param name="subscriptionName" required="true" cref="string" in="path">Name of the subscription</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscription"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscription.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateSubscription")]
        public async Task<IActionResult> CreateSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "gallery/applications/{appName}/subscriptions/{subscriptionName}")] HttpRequest req,
            string appName,
            string subscriptionName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateSubscription));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions", null, lunaHeaders))
                    {
                        var sub = await _galleryServiceClient.CreateLunaApplicationSubscription(appName, subscriptionName, lunaHeaders);
                        return new OkObjectResult(sub);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateSubscription));
                }
            }
        }

        /// <summary>
        /// Delete a subscription of a published Luna application
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="req">Http request</param>
        /// <response code="204">Success</response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteSubscription")]
        public async Task<IActionResult> DeleteSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetSubscription));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        await _galleryServiceClient.DeleteLunaApplicationSubscription(appName, subscriptionNameOrId, lunaHeaders);
                        return new NoContentResult();
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetSubscription));
                }
            }
        }

        /// <summary>
        /// Get a subscription of Luna application
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}</url>
        /// <param name="appName" required="true" cref="string" in="path">The name of the application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscription"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscription.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of subscription of Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetSubscription")]
        public async Task<IActionResult> GetSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetSubscription));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        var sub = await _galleryServiceClient.GetLunaApplicationSubscription(appName, subscriptionNameOrId, lunaHeaders);
                        return new OkObjectResult(sub);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetSubscription));
                }
            }
        }


        /// <summary>
        /// List all subscription of a Luna application for current user
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/subscriptions</url>
        /// <param name="appName" required="true" cref="string" in="path">The name of the application</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="LunaApplicationSubscription"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscription.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of subscription of Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListSubscriptions")]
        public async Task<IActionResult> ListSubscriptions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applications/{appName}/subscriptions")] HttpRequest req,
            string appName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListSubscriptions));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions", null, lunaHeaders))
                    {
                        var subList = await _galleryServiceClient.ListLunaApplicationSubscription(appName, lunaHeaders);
                        return new OkObjectResult(subList);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListSubscriptions));
                }
            }
        }

        /// <summary>
        /// Add a owner to the specified subscription
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/addOwner</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationSubscriptionOwner"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionOwner.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of owner of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscriptionOwner"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionOwner.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of owner of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("AddSubscriptionOwner")]
        public async Task<IActionResult> AddSubscriptionOwner(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/addOwner")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddSubscriptionOwner));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        var owner = await HttpUtils.DeserializeRequestBodyAsync<LunaApplicationSubscriptionOwner>(req);
                        owner = await _galleryServiceClient.AddLunaApplicationSubscriptionOwner(appName, 
                            subscriptionNameOrId, 
                            owner.UserId, 
                            owner.UserName, 
                            lunaHeaders);
                        return new OkObjectResult(owner);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AddSubscriptionOwner));
                }
            }
        }

        /// <summary>
        /// Remove a owner from the specified subscription
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/removeOwner</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationSubscriptionOwner"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionOwner.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of owner of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscriptionOwner"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionOwner.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of owner of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("RemoveSubscriptionOwner")]
        public async Task<IActionResult> RemoveSubscriptionOwner(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/removeOwner")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemoveSubscriptionOwner));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        var owner = await HttpUtils.DeserializeRequestBodyAsync<LunaApplicationSubscriptionOwner>(req);
                        owner = await _galleryServiceClient.RemoveLunaApplicationSubscriptionOwner(appName,
                            subscriptionNameOrId,
                            owner.UserId,
                            lunaHeaders);
                        return new OkObjectResult(owner);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RemoveSubscriptionOwner));
                }
            }
        }

        /// <summary>
        /// Regenerate key for the specified subscription
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/regenerateKey</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="key-name" required="true" cref="string" in="query">Name of the key</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscriptionKeys"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionKeys.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of keys of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("RegenerateSubscriptionKey")]
        public async Task<IActionResult> RegenerateSubscriptionKey(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/regenerateKey")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RegenerateSubscriptionKey));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        if (!req.Query.ContainsKey(GalleryServiceQueryParametersConstants.SUBCRIPTION_KEY_NAME_PARAM_NAME))
                        {
                            throw new LunaBadRequestUserException(
                                string.Format(ErrorMessages.MISSING_QUERY_PARAMETER, "key-name"),
                                UserErrorCode.MissingQueryParameter);
                        }
                        else
                        {
                            var keys = await _galleryServiceClient.RegenerateLunaApplicationSubscriptionKey(appName,
                                subscriptionNameOrId,
                                req.Query["key-name"].ToString(),
                                lunaHeaders);
                            return new OkObjectResult(keys);
                        }
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RegenerateSubscriptionKey));
                }
            }
        }

        /// <summary>
        /// Update notes for the specified subscription
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/updatenotes</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the Luna application</param>
        /// <param name="subscriptionNameOrId" required="true" cref="string" in="path">Name or id of the subscription</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationSubscriptionNotes"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionNotes.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of notes of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationSubscriptionNotes"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationSubscriptionNotes.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of notes of Luna application subscription
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateSubscriptionNotes")]
        public async Task<IActionResult> UpdateSubscriptionNotes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gallery/applications/{appName}/subscriptions/{subscriptionNameOrId}/updateNotes")] HttpRequest req,
            string appName,
            string subscriptionNameOrId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateSubscriptionNotes));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions/{subscriptionNameOrId}", null, lunaHeaders))
                    {
                        var notes = await HttpUtils.DeserializeRequestBodyAsync<LunaApplicationSubscriptionNotes>(req);
                        notes = await _galleryServiceClient.UpdateLunaApplicationSubscriptionNotes(appName,
                            subscriptionNameOrId,
                            notes.Notes,
                            lunaHeaders);
                        return new OkObjectResult(notes);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateSubscriptionNotes));
                }
            }
        }

        /// <summary>
        /// Register a publisher
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/gallery/applicationpublishers/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the webhook</param>
        /// <param name="req" in="body">
        ///     <see cref="ApplicationPublisher"/>
        ///     <example>
        ///         <value>
        ///             <see cref="ApplicationPublisher.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of application publisher
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="ApplicationPublisher"/>
        ///     <example>
        ///         <value>
        ///             <see cref="ApplicationPublisher.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of application publisher
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateApplicationPublisher")]
        public async Task<IActionResult> CreateApplicationPublisher(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "gallery/applicationpublishers/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateApplicationPublisher));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"applicationpublishers", null, lunaHeaders))
                    {
                        var publisher = await HttpUtils.DeserializeRequestBodyAsync<ApplicationPublisher>(req);

                        var result = await _galleryServiceClient.CreateApplicationPublisherAsync(name, publisher, lunaHeaders);

                        return new OkObjectResult(result);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateApplicationPublisher));
                }
            }
        }


        /// <summary>
        /// Update a publisher
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/gallery/applicationpublishers/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the webhook</param>
        /// <param name="req" in="body">
        ///     <see cref="ApplicationPublisher"/>
        ///     <example>
        ///         <value>
        ///             <see cref="ApplicationPublisher.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of application publisher
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="ApplicationPublisher"/>
        ///     <example>
        ///         <value>
        ///             <see cref="ApplicationPublisher.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of application publisher
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateApplicationPublisher")]
        public async Task<IActionResult> UpdateApplicationPublisher(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "gallery/applicationpublishers/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateApplicationPublisher));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"applicationpublishers", null, lunaHeaders))
                    {
                        var publisher = await HttpUtils.DeserializeRequestBodyAsync<ApplicationPublisher>(req);

                        var result = await _galleryServiceClient.UpdateApplicationPublisherAsync(name, publisher, lunaHeaders);

                        return new OkObjectResult(result);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateApplicationPublisher));
                }
            }
        }

        /// <summary>
        /// Delete a publisher
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/gallery/applicationpublishers/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the webhook</param>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteApplicationPublisher")]
        public async Task<IActionResult> DeleteApplicationPublisher(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "gallery/applicationpublishers/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateApplicationPublisher));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"applicationpublishers", null, lunaHeaders))
                    {

                        await _galleryServiceClient.DeleteApplicationPublisherAsync(name, lunaHeaders);

                        return new NoContentResult();
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateApplicationPublisher));
                }
            }
        }

        /// <summary>
        /// Get a publisher
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/gallery/applicationpublishers/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the webhook</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="ApplicationPublisher"/>
        ///     <example>
        ///         <value>
        ///             <see cref="ApplicationPublisher.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of application publisher
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetApplicationPublisher")]
        public async Task<IActionResult> GetApplicationPublisher(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applicationpublishers/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetApplicationPublisher));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"applicationpublishers", null, lunaHeaders))
                    {
                        var result = await _galleryServiceClient.GetApplicationPublisherAsync(name, lunaHeaders);

                        return new OkObjectResult(result);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetApplicationPublisher));
                }
            }
        }

        /// <summary>
        /// List publishers
        /// </summary>
        /// <group>ML Gallery</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/gallery/applicationpublishers</url>
        /// <param name="type" required="false" cref="string" in="query">Type of the publisher</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="ApplicationPublisher"/>
        ///     <example>
        ///         <value>
        ///             <see cref="ApplicationPublisher.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of application publisher
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListApplicationPublishers")]
        public async Task<IActionResult> ListApplicationPublishers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "gallery/applicationpublishers")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListApplicationPublishers));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"applicationpublishers", null, lunaHeaders))
                    {
                        string type = null;
                        if (req.Query.ContainsKey(GalleryServiceQueryParametersConstants.PUBLISHER_TYPE_PARAM_NAME))
                        {
                            type = req.Query[GalleryServiceQueryParametersConstants.PUBLISHER_TYPE_PARAM_NAME].ToString();
                        }

                        var result = await _galleryServiceClient.ListApplicationPublishersAsync(lunaHeaders, type);

                        return new OkObjectResult(result);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListApplicationPublishers));
                }
            }
        }
        #endregion


        #region signin CLI

        [FunctionName("GetAccessToken")]
        public async Task<IActionResult> GetAccessToken(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/accessToken")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetAccessToken));

                try
                {
                    HttpClient client = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/common/oauth2/token");
                    if (!req.Query.ContainsKey("device_code"))
                    {
                        throw new LunaBadRequestUserException("device_code is needed", UserErrorCode.MissingQueryParameter);
                    }

                    var deviceCode = req.Query["device_code"].ToString();

                    request.Content = new StringContent($"grant_type=device_code&client_id=04b07795-8ddb-461a-bbee-02f9e1bf7b46&resource=https%3A%2F%2Fmanagement.core.windows.net%2F&code={deviceCode}");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return new OkObjectResult(JObject.Parse(content));
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetAccessToken));
                }
            }
        }


        [FunctionName("GetDeviceCode")]
        public async Task<IActionResult> GetDeviceCode(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/deviceCode")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetDeviceCode));

                try
                {
                    HttpClient client = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/common/oauth2/devicecode?api-version-1.0");

                    request.Content = new StringContent("client_id=04b07795-8ddb-461a-bbee-02f9e1bf7b46&resource=https%3A%2F%2Fmanagement.core.windows.net%2F");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return new OkObjectResult(JObject.Parse(content));
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetDeviceCode));
                }
            }
        }
        #endregion

    }
}
