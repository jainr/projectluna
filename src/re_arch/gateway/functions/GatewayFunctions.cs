using Luna.Common.LoggingUtils;
using Luna.Common.Utils.HttpUtils;
using Luna.Common.Utils.LoggingUtils;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Common.Utils.RestClients;
using Luna.Gallery.Public.Client.Clients;
using Luna.Gallery.Public.Client.DataContracts;
using Luna.Publish.PublicClient.Clients;
using Luna.Publish.Public.Client.DataContract;
using Luna.PubSub.PublicClient.Clients;
using Luna.RBAC.Public.Client;
using Luna.RBAC.Public.Client.DataContracts;
using Luna.RBAC.Public.Client.Enums;
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
using Luna.PubSub.PublicClient;
using Luna.Partner.Public.Client;
using Newtonsoft.Json;

namespace Luna.Gateway.Functions
{
    public class GatewayFunctions
    {
        private readonly ILogger<GatewayFunctions> _logger;
        private readonly IPartnerServiceClient _partnerServiceClient;
        private readonly IRBACClient _rbacClient;
        private readonly IPublishServiceClient _publishServiceClient;
        private readonly IPubSubServiceClient _pubSubServiceClient;
        private readonly IGalleryServiceClient _galleryServiceClient;

        public GatewayFunctions(IPartnerServiceClient partnerServiceClient,
            IRBACClient rbacClient,
            IPublishServiceClient publishServiceClient,
            IPubSubServiceClient pubSubServiceClient,
            IGalleryServiceClient galleryServiceClient,
            ILogger<GatewayFunctions> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._rbacClient = rbacClient ?? throw new ArgumentNullException(nameof(rbacClient));
            this._publishServiceClient = publishServiceClient ?? throw new ArgumentNullException(nameof(publishServiceClient));
            this._partnerServiceClient = partnerServiceClient ?? throw new ArgumentNullException(nameof(partnerServiceClient));
            this._pubSubServiceClient = pubSubServiceClient ?? throw new ArgumentNullException(nameof(pubSubServiceClient));
            this._galleryServiceClient = galleryServiceClient ?? throw new ArgumentNullException(nameof(galleryServiceClient));
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
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"rbac", null, lunaHeaders))
                    {
                        return new OkObjectResult(req.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].ToString());
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

        #region pubsub

        /// <summary>
        /// Get event store connection string
        /// </summary>
        /// <group>Event Store</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/eventstores/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the event store</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="EventStoreInfo"/>
        ///     <example>
        ///         <value>
        ///             <see cref="EventStoreInfo.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of event store info
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
        [FunctionName("GetEventStoreConnectionString")]
        public async Task<IActionResult> GetEventStoreConnectionString(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/eventstores/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetEventStoreConnectionString));

                try
                {

                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/eventstores", null, lunaHeaders))
                    {
                        EventStoreInfo result = await _pubSubServiceClient.GetEventStoreConnectionStringAsync(name, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.GetEventStoreConnectionString));
                }
            }
        }
        #endregion

        #region publish - Azure Marketplace offer

        /// <summary>
        /// Create an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req" in="body">
        ///     <see cref="AzureMarketplaceOffer"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplaceOffer.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="AzureMarketplaceOffer"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplaceOffer.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateAzureMarketplaceOffer")]
        public async Task<IActionResult> CreateAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "marketplace/offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateAzureMarketplaceOffer));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        RBACActions.CREATE_MARKETPLACE_OFFER,
                        lunaHeaders))
                    {
                        var content = await HttpUtils.DeserializeRequestBodyAsync<AzureMarketplaceOffer>(req);
                        var result = await _publishServiceClient.CreateMarketplaceOfferAsync(offerId, content, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.CreateAzureMarketplaceOffer));
                }
            }
        }

        /// <summary>
        /// Update an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req" in="body">
        ///     <see cref="AzureMarketplaceOffer"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplaceOffer.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="AzureMarketplaceOffer"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplaceOffer.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateAzureMarketplaceOffer")]
        public async Task<IActionResult> UpdateAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "marketplace/offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateAzureMarketplaceOffer));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        null,
                        lunaHeaders))
                    {
                        var content = await HttpUtils.DeserializeRequestBodyAsync<AzureMarketplaceOffer>(req);
                        var result = await _publishServiceClient.UpdateMarketplaceOfferAsync(offerId, content, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.UpdateAzureMarketplaceOffer));
                }
            }
        }


        /// <summary>
        /// Publish an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/publish</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="AzureMarketplaceOffer"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplaceOffer.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("PublishAzureMarketplaceOffer")]
        public async Task<IActionResult> PublishAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "marketplace/offers/{offerId}/publish")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.PublishAzureMarketplaceOffer));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        null,
                        lunaHeaders))
                    {
                        var result = await _publishServiceClient.PublishMarketplaceOfferAsync(offerId, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.PublishAzureMarketplaceOffer));
                }
            }
        }

        /// <summary>
        /// Delete an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteAzureMarketplaceOffer")]
        public async Task<IActionResult> DeleteAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "marketplace/offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteAzureMarketplaceOffer));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        null,
                        lunaHeaders))
                    {
                        await _publishServiceClient.DeleteMarketplaceOfferAsync(offerId, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.DeleteAzureMarketplaceOffer));
                }
            }
        }

        /// <summary>
        /// Get an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="AzureMarketplaceOffer"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplaceOffer.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetAzureMarketplaceOffer")]
        public async Task<IActionResult> GetAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "marketplace/offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetAzureMarketplaceOffer));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        null,
                        lunaHeaders))
                    {
                        var result = await _publishServiceClient.GetMarketplaceOfferAsync(offerId, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.GetAzureMarketplaceOffer));
                }
            }
        }

        /// <summary>
        /// List Azure Marketplace SaaS offers
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/marketplace/offers</url>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="AzureMarketplaceOffer"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplaceOffer.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListAzureMarketplaceOffers")]
        public async Task<IActionResult> ListAzureMarketplaceOffers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "marketplace/offers")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListAzureMarketplaceOffers));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers",
                        RBACActions.LIST_MARKETPLACE_OFFER,
                        lunaHeaders))
                    {
                        var result = await _publishServiceClient.ListMarketplaceOffersAsync(lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.ListAzureMarketplaceOffers));
                }
            }
        }

        /// <summary>
        /// Create a plan in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req" in="body">
        ///     <see cref="AzureMarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplacePlan.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="AzureMarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplacePlan.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateAzureMarketplacePlan")]
        public async Task<IActionResult> CreateAzureMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "marketplace/offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateAzureMarketplacePlan));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        null,
                        lunaHeaders))
                    {
                        var content = await HttpUtils.DeserializeRequestBodyAsync<AzureMarketplacePlan>(req);
                        var result = await _publishServiceClient.CreateMarketplacePlanAsync(offerId, planId, content, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.CreateAzureMarketplacePlan));
                }
            }
        }

        /// <summary>
        /// Update a plan in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req" in="body">
        ///     <see cref="AzureMarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplacePlan.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="AzureMarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplacePlan.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateAzureMarketplacePlan")]
        public async Task<IActionResult> UpdateAzureMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "marketplace/offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateAzureMarketplacePlan));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        null,
                        lunaHeaders))
                    {
                        var content = await HttpUtils.DeserializeRequestBodyAsync<AzureMarketplacePlan>(req);
                        var result = await _publishServiceClient.UpdateMarketplacePlanAsync(offerId, planId, content, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.UpdateAzureMarketplacePlan));
                }
            }
        }

        /// <summary>
        /// Delete a plan in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteAzureMarketplacePlan")]
        public async Task<IActionResult> DeleteAzureMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "marketplace/offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteAzureMarketplacePlan));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        null,
                        lunaHeaders))
                    {
                        await _publishServiceClient.DeleteMarketplacePlanAsync(offerId, planId, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.DeleteAzureMarketplacePlan));
                }
            }
        }

        /// <summary>
        /// Get a plan in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="AzureMarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplacePlan.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetAzureMarketplacePlan")]
        public async Task<IActionResult> GetAzureMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "marketplace/offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetAzureMarketplacePlan));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        null,
                        lunaHeaders))
                    {
                        var result = await _publishServiceClient.GetMarketplacePlanAsync(offerId, planId, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.GetAzureMarketplacePlan));
                }
            }
        }

        /// <summary>
        /// List plans in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/plans</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is<see cref="AzureMarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMarketplacePlan.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListAzureMarketplacePlans")]
        public async Task<IActionResult> ListAzureMarketplacePlans(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "marketplace/offers/{offerId}/plans")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListAzureMarketplacePlans));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId,
                        $"/offers/{offerId}",
                        null,
                        lunaHeaders))
                    {
                        var result = await _publishServiceClient.ListMarketplacePlansAsync(offerId, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.ListAzureMarketplacePlans));
                }
            }
        }


        /// <summary>
        /// Get offer parameters
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/offerparameters</url>
        /// <param name="offerId" required="true" cref="string" in="path">The offer ID</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="MarketplaceOfferParameter"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceOfferParameter.example"/>
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
        [FunctionName("GetMarketplaceOfferParameters")]
        public async Task<IActionResult> GetMarketplaceOfferParameters(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "marketplace/offers/{offerId}/offerparameters")]
            HttpRequest req,
            string offerId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMarketplaceOfferParameters));

                try
                {
                    // This should be a public endpoint
                    var result = await _galleryServiceClient.GetOfferParametersAsync(offerId, lunaHeaders);
                    return new OkObjectResult(result);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetMarketplaceOfferParameters));
                }
            }
        }

        /// <summary>
        /// Resolve Azure Marketplace subscription from token
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/marketplace/subscriptions/resolveToken</url>
        /// <param name="req" in="body"><see cref="string"/>Token</param>
        /// <response code="200">
        ///     <see cref="MarketplaceSubscription"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscription.example"/>
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
                    // This should be a public endpoint
                    var token = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _galleryServiceClient.ResolveMarketplaceTokenAsync(token, lunaHeaders);

                    // TODO: verify the ownership

                    return new OkObjectResult(result);
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
        ///     <see cref="MarketplaceSubscription"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscription.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a marketplace subscription
        ///         </summary>
        ///     </example>
        ///     The subscription
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplaceSubscription"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscription.example"/>
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
                    var content = await HttpUtils.DeserializeRequestBodyAsync<MarketplaceSubscription>(req);
                    var result = await _galleryServiceClient.CreateMarketplaceSubscriptionAsync(subscriptionId, content, lunaHeaders);
                    return new OkObjectResult(result);
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

        #endregion

        #region publish - Luna application

        /// <summary>
        /// Regenerate application master key
        /// </summary>
        /// <group>Applications</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/manage/applications/{name}/regeneratemasterkeys</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="key-name" required="true" cref="string" in="query">Name of key</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationMasterKeys"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationMasterKeys.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application master keys
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
        [FunctionName("RegenerateLunaApplicationMasterKeys")]
        public async Task<IActionResult> RegenerateLunaApplicationMasterKeys(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/applications/{name}/regeneratemasterkeys")] HttpRequest req,
        string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RegenerateLunaApplicationMasterKeys));

                try
                {
                    var keyName = "";
                    if (req.Query.ContainsKey(PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME))
                    {
                        keyName = req.Query[PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME].ToString();
                    }
                    else
                    {
                        throw new LunaBadRequestUserException(
                            string.Format(ErrorMessages.MISSING_QUERY_PARAMETER, PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME),
                            UserErrorCode.MissingQueryParameter);
                    }

                    if (!keyName.Equals(PublishQueryParameterConstants.PRIMARY_KEY_NAME) &&
                        !keyName.Equals(PublishQueryParameterConstants.SECONDARY_KEY_NAME))
                    {
                        throw new LunaBadRequestUserException(
                            string.Format(ErrorMessages.INVALID_QUERY_PARAMETER_VALUE, PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME),
                            UserErrorCode.InvalidParameter);
                    }

                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                    {
                        var result = await _publishServiceClient.RegenerateApplicationMasterKeys(name, keyName, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.RegenerateLunaApplicationMasterKeys));
                }
            }

        }

        /// <summary>
        /// List applications
        /// </summary>
        /// <group>Applications</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/applications</url>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="LunaApplication"/>
        ///     Success
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListLunaApplications")]
        public async Task<IActionResult> ListLunaApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/applications")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListLunaApplications));

                try
                {
                    if (string.IsNullOrEmpty(lunaHeaders.UserId))
                    {
                        throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                    }

                    var rbacResult = await _rbacClient.GetRBACQueryResult(lunaHeaders.UserId, RBACActions.LIST_APPLICATIONS, null, lunaHeaders);

                    if (rbacResult.CanAccess)
                    {
                        var result = await _publishServiceClient.ListLunaApplications(rbacResult.Role.Equals(RBACRole.SystemAdmin.ToString()), lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.ListLunaApplications));
                }
            }
        }

        /// <summary>
        /// Get application master keys
        /// </summary>
        /// <group>Applications</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/applications/{name}/masterkeys</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationMasterKeys"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationMasterKeys.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application master keys
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
        [FunctionName("GetLunaApplicationMasterKeys")]
        public async Task<IActionResult> GetLunaApplicationMasterKeys(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/applications/{name}/masterkeys")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetLunaApplicationMasterKeys));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                    {
                        var result = await _publishServiceClient.GetApplicationMasterKeys(name, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.GetLunaApplicationMasterKeys));
                }
            }
        }


        /// <summary>
        /// Create a new Luna application
        /// </summary>
        /// <group>Applications</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/manage/applications/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna application properties
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna application properties
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
        [FunctionName("CreateLunaApplication")]
        public async Task<IActionResult> CreateLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateLunaApplication));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", RBACActions.CREATE_NEW_APPLICATION, lunaHeaders))
                    {
                        var content = await HttpUtils.GetRequestBodyAsync(req);
                        var result = await _publishServiceClient.CreateLunaApplication(name, content, lunaHeaders);
                        if (await _rbacClient.AddApplicationOwner(lunaHeaders.UserId, $"/applications/{name}", lunaHeaders))
                        {
                            return new OkObjectResult(result);
                        }
                        else
                        {
                            // If failed to add owner, delete the application and throw exception
                            await _publishServiceClient.DeleteLunaApplication(name, lunaHeaders);
                            throw new LunaServerException($"Failed to add ownership for application {name}. Owner user id is {lunaHeaders.UserId}.");
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
                    _logger.LogMethodEnd(nameof(this.CreateLunaApplication));
                }
            }
        }

        /// <summary>
        /// Update a Luna application
        /// </summary>
        /// <group>Applications</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/manage/applications/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna application properties
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna application properties
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
        [FunctionName("UpdateLunaApplication")]
        public async Task<IActionResult> UpdateLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateLunaApplication));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                    {
                        var content = await HttpUtils.GetRequestBodyAsync(req);
                        var result = await _publishServiceClient.UpdateLunaApplication(name, content, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.UpdateLunaApplication));
                }
            }

        }


        /// <summary>
        /// Delete a Luna application
        /// </summary>
        /// <group>Applications</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/manage/applications/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="req">Http request</param>
        /// <response code="204">Success</response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteLunaApplication")]
        public async Task<IActionResult> DeleteLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteLunaApplication));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                    {
                        await _publishServiceClient.DeleteLunaApplication(name, lunaHeaders);
                        await _rbacClient.RemoveApplicationOwner(lunaHeaders.UserId, $"/applications/{name}", lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.DeleteLunaApplication));
                }
            }

        }

        /// <summary>
        /// Create an API in a Luna application
        /// </summary>
        /// <group>Applications</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/manage/applications/{appName}/apis/{apiName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of the API</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseLunaAPIProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseLunaAPIProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna API properties
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseLunaAPIProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseLunaAPIProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna API properties
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
        [FunctionName("CreateLunaAPI")]
        public async Task<IActionResult> CreateLunaAPI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateLunaAPI));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                    {
                        var content = await HttpUtils.GetRequestBodyAsync(req);
                        var result = await _publishServiceClient.CreateLunaAPI(appName, apiName, content, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.CreateLunaAPI));
                }
            }

        }

        /// <summary>
        /// Update an API in a Luna application
        /// </summary>
        /// <group>Applications</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/manage/applications/{appName}/apis/{apiName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of the API</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseLunaAPIProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseLunaAPIProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna API properties
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseLunaAPIProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseLunaAPIProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna API properties
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
        [FunctionName("UpdateLunaAPI")]
        public async Task<IActionResult> UpdateLunaAPI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateLunaAPI));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                    {
                        var content = await HttpUtils.GetRequestBodyAsync(req);
                        var result = await _publishServiceClient.UpdateLunaAPI(appName, apiName, content, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.UpdateLunaAPI));
                }
            }

        }

        /// <summary>
        /// Delete an API from a Luna application
        /// </summary>
        /// <group>Applications</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/manage/applications/{appName}/apis/{apiName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of the API</param>
        /// <param name="req">Http request</param>
        /// <response code="204">Success</response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteLunaAPI")]
        public async Task<IActionResult> DeleteLunaAPI(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteLunaAPI));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                    {
                        await _publishServiceClient.DeleteLunaAPI(appName, apiName, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.DeleteLunaAPI));
                }
            }

        }

        /// <summary>
        /// Create a new version in a Luna API
        /// </summary>
        /// <group>Applications</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/manage/applications/{appName}/apis/{apiName}/versions/{versionName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of the API</param>
        /// <param name="versionName" required="true" cref="string" in="path">Name of the Version</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseAPIVersionProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseAPIVersionProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna API version
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseAPIVersionProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseAPIVersionProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna API version
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
        [FunctionName("CreateLunaAPIVersion")]
        public async Task<IActionResult> CreateLunaAPIVersion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/applications/{appName}/apis/{apiName}/versions/{versionName}")] HttpRequest req,
            string appName,
            string apiName,
            string versionName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateLunaAPIVersion));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                    {
                        var content = await HttpUtils.GetRequestBodyAsync(req);
                        var result = await _publishServiceClient.CreateLunaAPIVersion(appName, apiName, versionName, content, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.CreateLunaAPIVersion));
                }
            }

        }

        /// <summary>
        /// Update a new version in a Luna API
        /// </summary>
        /// <group>Applications</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/manage/applications/{appName}/apis/{apiName}/versions/{versionName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of the API</param>
        /// <param name="versionName" required="true" cref="string" in="path">Name of the Version</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseAPIVersionProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseAPIVersionProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna API version
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseAPIVersionProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseAPIVersionProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna API version
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
        [FunctionName("UpdateLunaAPIVersion")]
        public async Task<IActionResult> UpdateLunaAPIVersion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/applications/{appName}/apis/{apiName}/versions/{versionName}")] HttpRequest req,
            string appName,
            string apiName,
            string versionName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateLunaAPIVersion));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                    {
                        var content = await HttpUtils.GetRequestBodyAsync(req);
                        var result = await _publishServiceClient.UpdateLunaAPIVersion(appName, apiName, versionName, content, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.UpdateLunaAPIVersion));
                }
            }

        }

        /// <summary>
        /// Delete a new version in a Luna API
        /// </summary>
        /// <group>Applications</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/manage/applications/{appName}/apis/{apiName}/versions/{versionName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of the API</param>
        /// <param name="versionName" required="true" cref="string" in="path">Name of the Version</param>
        /// <param name="req">Http request</param>
        /// <response code="204">Success</response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteLunaAPIVersion")]
        public async Task<IActionResult> DeleteLunaAPIVersion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/applications/{appName}/apis/{apiName}/versions/{versionName}")] HttpRequest req,
            string appName,
            string apiName,
            string versionName)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteLunaAPIVersion));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{appName}", null, lunaHeaders))
                    {
                        await _publishServiceClient.DeleteLunaAPIVersion(appName, apiName, versionName, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.DeleteLunaAPIVersion));
                }
            }

        }

        /// <summary>
        /// Publish a Luna application
        /// </summary>
        /// <group>Applications</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/manage/applications/{name}/publish</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="req">Http request</param>
        /// <response code="204">Success</response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("PublishLunaApplication")]
        public async Task<IActionResult> PublishLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/applications/{name}/publish")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.PublishLunaApplication));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                    {
                        var comments = "No comment.";
                        if (req.Query.ContainsKey("comments"))
                        {
                            comments = req.Query["comments"].ToString();
                        }
                        await _publishServiceClient.PublishLunaApplication(name, comments, lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.PublishLunaApplication));
                }
            }

        }

        /// <summary>
        /// Get a Luna application
        /// </summary>
        /// <group>Applications</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/applications/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the application</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of luna application properties
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
        [FunctionName("GetLunaApplication")]
        public async Task<IActionResult> GetLunaApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetLunaApplication));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"/applications/{name}", null, lunaHeaders))
                    {
                        var result = await _publishServiceClient.GetLunaApplication(name, lunaHeaders);

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
                    _logger.LogMethodEnd(nameof(this.GetLunaApplication));
                }
            }

        }
        #endregion

        #region rbac

        /// <summary>
        /// Remove role assignment
        /// </summary>
        /// <group>RBAC</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/manage/rbac/roleassignments/remove</url>
        /// <param name="req" in="body">
        ///     <see cref="RoleAssignment"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignment.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of role assignment
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="204">Success</response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("RemoveRoleAssignment")]
        public async Task<IActionResult> RemoveRoleAssignment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/rbac/roleassignments/remove")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemoveRoleAssignment));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"rbac", null, lunaHeaders))
                    {
                        var roleAssignment = await HttpUtils.DeserializeRequestBodyAsync<RoleAssignment>(req);

                        if (roleAssignment.Uid.Equals(lunaHeaders.UserId) &&
                            roleAssignment.Role.Equals(RBACRole.SystemAdmin.ToString()))
                        {
                            throw new LunaConflictUserException(ErrorMessages.CAN_NOT_REMOVE_YOUR_OWN_ACCOUNT_FROM_ADMN);
                        }

                        var result = await _rbacClient.RemoveRoleAssignment(roleAssignment, lunaHeaders);

                        if (result)
                        {
                            return new NoContentResult();
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
                    _logger.LogMethodEnd(nameof(this.RemoveRoleAssignment));
                }
            }

        }

        /// <summary>
        /// Add role assignment
        /// </summary>
        /// <group>RBAC</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/manage/rbac/roleassignments/add</url>
        /// <param name="req" in="body">
        ///     <see cref="RoleAssignment"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignment.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of role assignment
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="RoleAssignment"/>
        ///     <example>
        ///         <value>
        ///             <see cref="RoleAssignment.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of role assignment
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListRoleAssignments")]
        public async Task<IActionResult> ListRoleAssignments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/rbac/roleassignments")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListRoleAssignments));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"rbac", null, lunaHeaders))
                    {
                        var result = await _rbacClient.ListRoleAssignments(lunaHeaders);
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
                    _logger.LogMethodEnd(nameof(this.ListRoleAssignments));
                }
            }
        }

        /// <summary>
        /// List role assignments
        /// </summary>
        /// <group>RBAC</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/manage/rbac/roleassignments</url>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("AddRoleAssignment")]
        public async Task<IActionResult> AddRoleAssignment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "manage/rbac/roleassignments/add")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddRoleAssignment));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"rbac", null, lunaHeaders))
                    {
                        var roleAssignment = await HttpUtils.DeserializeRequestBodyAsync<RoleAssignment>(req);
                        var result = await _rbacClient.AddRoleAssignment(roleAssignment, lunaHeaders);

                        if (result)
                        {
                            return new NoContentResult();
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
                    _logger.LogMethodEnd(nameof(this.AddRoleAssignment));
                }
            }
        }
        #endregion

        #region partner

        /// <summary>
        /// Register a partner service
        /// </summary>
        /// <group>Partner Service</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req" in="body">
        ///     <see cref="BasePartnerServiceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BasePartnerServiceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of partner service configuration
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BasePartnerServiceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BasePartnerServiceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of partner service configuration
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
        [FunctionName("AddPartnerService")]
        public async Task<IActionResult> AddPartnerService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/partnerservices/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddPartnerService));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"partnerservices", null, lunaHeaders))
                    {
                        var config = await ParsePartnerServiceConfigurationAsync(req);

                        await _partnerServiceClient.RegisterPartnerServiceAsync(name, config, lunaHeaders);
                        return new OkObjectResult(config);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AddPartnerService));
                }
            }
        }

        /// <summary>
        /// Update a partner service
        /// </summary>
        /// <group>Partner Service</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req" in="body">
        ///     <see cref="BasePartnerServiceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BasePartnerServiceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of partner service configuration
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BasePartnerServiceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BasePartnerServiceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of partner service configuration
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
        [FunctionName("UpdatePartnerService")]
        public async Task<IActionResult> UpdatePartnerService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/partnerservices/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdatePartnerService));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"partnerservices", null, lunaHeaders))
                    {
                        var config = await ParsePartnerServiceConfigurationAsync(req);
                        await _partnerServiceClient.UpdatePartnerServiceAsync(name, config, lunaHeaders);
                        return new OkObjectResult(config);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdatePartnerService));
                }
            }

        }


        /// <summary>
        /// Remove a registered partner service
        /// </summary>
        /// <group>Partner Service</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req">Http request</param>
        /// <response code="204">Success</response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("RemovePartnerService")]
        public async Task<IActionResult> RemovePartnerService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/partnerservices/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemovePartnerService));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"partnerservices", null, lunaHeaders))
                    {
                        if (await _partnerServiceClient.DeletePartnerServiceAsync(name, lunaHeaders))
                        {
                            return new NoContentResult();
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
                    _logger.LogMethodEnd(nameof(this.RemovePartnerService));
                }
            }

        }

        /// <summary>
        /// List all partner services
        /// </summary>
        /// <group>Partner Service</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices</url>
        /// <param name="req">Http request</param>
        /// <param name="type" required="true" cref="string" in="query">Type of partner service</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="PartnerService"/>
        ///     <example>
        ///         <value>
        ///             <see cref="PartnerService.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure ML workspace as partner services
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
        [FunctionName("ListPartnerServices")]
        public async Task<IActionResult> ListPartnerServices(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListPartnerServices));

                try
                {
                    string type = null;
                    if (req.Query.ContainsKey(PartnerQueryParameterConstats.PARTNER_SERVICE_TYPE_QUERY_PARAM_NAME))
                    {
                        type = req.Query[PartnerQueryParameterConstats.PARTNER_SERVICE_TYPE_QUERY_PARAM_NAME].ToString();
                    }

                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", null, lunaHeaders))
                    {
                        var config = await _partnerServiceClient.ListPartnerServicesAsync(lunaHeaders, type);
                        return new OkObjectResult(config);
                    }

                    throw new LunaUnauthorizedUserException(
                        string.Format(ErrorMessages.CAN_NOT_PERFORM_OPERATION));
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListPartnerServices));
                }
            }

        }

        /// <summary>
        /// Get a partner service
        /// </summary>
        /// <group>Partner Service</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="BasePartnerServiceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BasePartnerServiceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of partner service configuration
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
        [FunctionName("GetPartnerService")]
        public async Task<IActionResult> GetPartnerService(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetPartnerService));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", null, lunaHeaders))
                    {
                        var config = await _partnerServiceClient.GetPartnerServiceConfigurationAsync(name, lunaHeaders);
                        return new OkObjectResult(config);
                    }

                    throw new LunaUnauthorizedUserException(
                        string.Format(ErrorMessages.PARTNER_SERVICE_DOES_NOT_EXIST, name));
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPartnerService));
                }
            }
        }

        /// <summary>
        /// List supported ML host service types
        /// </summary>
        /// <group>Partner Service</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/metadata/hostservicetypes</url>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="ServiceType"/>
        ///     <example>
        ///         <value>
        ///             <see cref="ServiceType.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of service type
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
        [FunctionName("ListMLHostServiceTypes")]
        public async Task<IActionResult> ListMLHostServiceTypes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/metadata/hostservicetypes")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMLHostServiceTypes));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", RBACActions.READ_PARTNER_SERVICES, lunaHeaders))
                    {
                        var types = await _partnerServiceClient.GetMLHostServiceTypes(lunaHeaders);
                        return new OkObjectResult(types);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListMLHostServiceTypes));
                }
            }
        }

        /// <summary>
        /// List supported ML component types by a partner service
        /// </summary>
        /// <group>Partner Service</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/metadata/{serviceType}/mlcomponenttypes</url>
        /// <param name="serviceType" required="true" cref="string" in="path">The type of partner service</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="ComponentType"/>
        ///     <example>
        ///         <value>
        ///             <see cref="ComponentType.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of component type
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
        [FunctionName("ListMLComponentTypes")]
        public async Task<IActionResult> ListMLComponentTypes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/metadata/{serviceType}/mlcomponenttypes")] HttpRequest req,
            string serviceType)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMLComponentTypes));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", RBACActions.READ_PARTNER_SERVICES, lunaHeaders))
                    {
                        var types = await _partnerServiceClient.GetMLComponentTypes(serviceType, lunaHeaders);
                        return new OkObjectResult(types);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListMLComponentTypes));
                }
            }
        }

        /// <summary>
        /// List specified type of ML components hosted by a partner service
        /// </summary>
        /// <group>Partner Service</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/{serviceName}/mlcomponents/{componentType}</url>
        /// <param name="serviceName" required="true" cref="string" in="path">The name of partner service</param>
        /// <param name="componentType" required="true" cref="string" in="path">The type of the component</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="BaseMLComponent"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseMLComponent.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of component type
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
        [FunctionName("ListMLComponents")]
        public async Task<IActionResult> ListMLComponents(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/{serviceName}/mlcomponents/{componentType}")] HttpRequest req,
            string serviceName,
            string componentType)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMLComponents));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", RBACActions.READ_PARTNER_SERVICES, lunaHeaders))
                    {
                        var types = await _partnerServiceClient.GetMLComponents(serviceName, componentType, lunaHeaders);
                        return new OkObjectResult(types);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListMLComponents));
                }
            }
        }

        /// <summary>
        /// List supported ML compute service types
        /// </summary>
        /// <group>Partner Service (deprecated)</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/metadata/computeservicetypes</url>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="ServiceType"/>
        ///     <example>
        ///         <value>
        ///             <see cref="ServiceType.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of service type
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
        [FunctionName("ListMLComputeServiceTypes")]
        public async Task<IActionResult> ListMLComputeServiceTypes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/metadata/computeservicetypes")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMLComputeServiceTypes));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", RBACActions.READ_PARTNER_SERVICES, lunaHeaders))
                    {
                        var types = await _partnerServiceClient.GetMLComputeServiceTypes(lunaHeaders);
                        return new OkObjectResult(types);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListMLComputeServiceTypes));
                }
            }
        }

        #endregion
        #region Partner Services (deprecated)

        /// <summary>
        /// Add Azure ML workspace as a partner service
        /// </summary>
        /// <group>Partner Service - deprecated</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/azureml/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req" in="body">
        ///     <see cref="AzureMLWorkspaceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMLWorkspaceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure ML workspace configuration
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="AzureMLWorkspaceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMLWorkspaceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure ML workspace configuration
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
        [FunctionName("AddAzureMLService")]
        public async Task<IActionResult> AddAzureMLService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddAzureMLService));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"partnerservices", null, lunaHeaders))
                    {
                        var config = await HttpUtils.DeserializeRequestBodyAsync<AzureMLWorkspaceConfiguration>(req);
                        await _partnerServiceClient.RegisterAzureMLWorkspace(name, config, lunaHeaders);
                        return new OkObjectResult(config);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AddAzureMLService));
                }
            }
        }

        /// <summary>
        /// Update Azure ML workspace as a partner service
        /// </summary>
        /// <group>Partner Service - deprecated)</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/azureml/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req" in="body">
        ///     <see cref="AzureMLWorkspaceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMLWorkspaceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure ML workspace configuration
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="AzureMLWorkspaceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMLWorkspaceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure ML workspace configuration
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
        [FunctionName("UpdateAzureMLService")]
        public async Task<IActionResult> UpdateAzureMLService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateAzureMLService));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"partnerservices", null, lunaHeaders))
                    {
                        var config = await HttpUtils.DeserializeRequestBodyAsync<AzureMLWorkspaceConfiguration>(req);
                        await _partnerServiceClient.UpdateAzureMLWorkspace(name, config, lunaHeaders);
                        return new OkObjectResult(config);
                    }

                    throw new LunaUnauthorizedUserException(ErrorMessages.CAN_NOT_PERFORM_OPERATION);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateAzureMLService));
                }
            }

        }


        /// <summary>
        /// Remove Azure ML workspace as a partner service
        /// </summary>
        /// <group>Partner Service - deprecated</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/azureml/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req">Http request</param>
        /// <response code="204">Success</response>
        /// <security type="http" name="http-bearer">
        ///     <description>Test security</description>
        ///     <scheme>bearer</scheme>
        ///     <bearerFormat>JWT</bearerFormat>
        /// </security>
        /// <returns></returns>
        [FunctionName("RemoveAzureMLService")]
        public async Task<IActionResult> RemoveAzureMLService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemoveAzureMLService));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"partnerservices", null, lunaHeaders))
                    {
                        if (await _partnerServiceClient.DeleteAzureMLWorkspace(name, lunaHeaders))
                        {
                            return new NoContentResult();
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
                    _logger.LogMethodEnd(nameof(this.RemoveAzureMLService));
                }
            }

        }

        /// <summary>
        /// List Azure ML workspaces registered as a partner service
        /// </summary>
        /// <group>Partner Service - deprecated</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/azureml</url>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="PartnerService"/>
        ///     <example>
        ///         <value>
        ///             <see cref="PartnerService.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure ML workspace as partner services
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
        [FunctionName("ListAzureMLPartnerServices")]
        public async Task<IActionResult> ListAzureMLPartnerServices(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/azureml")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListAzureMLPartnerServices));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", null, lunaHeaders))
                    {
                        var config = await _partnerServiceClient.ListAzureMLWorkspaces(lunaHeaders);
                        return new OkObjectResult(config);
                    }

                    throw new LunaUnauthorizedUserException(
                        string.Format(ErrorMessages.CAN_NOT_PERFORM_OPERATION));
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListAzureMLPartnerServices));
                }
            }

        }

        /// <summary>
        /// Get an Azure ML workspace as a partner service
        /// </summary>
        /// <group>Partner Service - deprecated</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/manage/partnerservices/azureml/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="AzureMLWorkspaceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="AzureMLWorkspaceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure ML workspace configuration
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
        [FunctionName("GetAzureMLService")]
        public async Task<IActionResult> GetAzureMLService(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/partnerservices/azureml/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetAzureMLService));

                try
                {
                    if (!string.IsNullOrEmpty(lunaHeaders.UserId) &&
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, "partnerservices", null, lunaHeaders))
                    {
                        var config = await _partnerServiceClient.GetAzureMLWorkspaceConfiguration(name, lunaHeaders);
                        return new OkObjectResult(config);
                    }

                    throw new LunaUnauthorizedUserException(
                        string.Format(ErrorMessages.PARTNER_SERVICE_DOES_NOT_EXIST, name));
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetAzureMLService));
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
                        await this._rbacClient.CanAccess(lunaHeaders.UserId, $"subscriptions", "List", lunaHeaders))
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
        #endregion

        private async Task<BasePartnerServiceConfiguration> ParsePartnerServiceConfigurationAsync(HttpRequest req)
        {
            var requestBody = await HttpUtils.GetRequestBodyAsync(req);

            var config = JsonConvert.DeserializeObject<BasePartnerServiceConfiguration>(requestBody, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            if (config.Type.Equals(PartnerServiceType.AzureML.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                config = JsonConvert.DeserializeObject<AzureMLWorkspaceConfiguration>(requestBody, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            else if (config.Type.Equals(PartnerServiceType.GitHub.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                config = JsonConvert.DeserializeObject<GitHubPartnerServiceConfiguration>(requestBody, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            else if (config.Type.Equals(PartnerServiceType.AzureSynapse.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                config = JsonConvert.DeserializeObject<AzureSynapseWorkspaceConfiguration>(requestBody, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            else
            {
                throw new LunaBadRequestUserException(string.Format(ErrorMessages.INVALID_PARTNER_SERVICE_TYPE),
                    UserErrorCode.InvalidParameter, nameof(config.Type));
            }

            return config;
        }
    }
}
