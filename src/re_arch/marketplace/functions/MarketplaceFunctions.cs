using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Luna.Marketplace.Clients;
using System.Collections.Generic;
using Luna.Marketplace.Data;
using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using Luna.PubSub.Public.Client;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Luna.Marketplace.Functions
{
    /// <summary>
    /// The service maintains all Luna application, APIs and API versions
    /// </summary>
    public class MarketplaceFunctions
    {
        private readonly IOfferEventContentGenerator _offerEventGenerator;
        private readonly IOfferEventProcessor _offerEventProcessor;
        private readonly ISqlDbContext _dbContext;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;
        private readonly IPubSubServiceClient _pubSubClient;
        private readonly IAzureMarketplaceSaaSClient _marketplaceClient;
        private readonly IMarketplaceFunctionsImpl _marketplaceFunction;
        private readonly ILogger<MarketplaceFunctions> _logger;

        public MarketplaceFunctions(IOfferEventProcessor offerEventProcessor,
            IOfferEventContentGenerator offerEventGenerator,
            ISqlDbContext dbContext,
            IAzureKeyVaultUtils keyVaultUtils,
            IPubSubServiceClient pubSubClient,
            IAzureMarketplaceSaaSClient marketplaceClient,
            IMarketplaceFunctionsImpl marketplaceFunction,
            ILogger<MarketplaceFunctions> logger)
        {
            this._offerEventGenerator = offerEventGenerator ?? throw new ArgumentNullException(nameof(offerEventGenerator));
            this._offerEventProcessor = offerEventProcessor ?? throw new ArgumentNullException(nameof(offerEventProcessor));
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
            this._pubSubClient = pubSubClient ?? throw new ArgumentNullException(nameof(pubSubClient));
            this._marketplaceClient = marketplaceClient ?? throw new ArgumentNullException(nameof(marketplaceClient));
            this._marketplaceFunction = marketplaceFunction ?? throw new ArgumentNullException(nameof(marketplaceFunction));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Azure Marketplace SaaS offers

        /// <summary>
        /// Create or update an Azure Marketplace SaaS offer from template
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/template</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req" in="body">
        ///     <see cref="MarketplaceOffer"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceOffer.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer template
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplaceOffer"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceOffer.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer template
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateOrUpdateAzureMarketplaceOfferFromTemplate")]
        public async Task<IActionResult> CreateOrUpdateAzureMarketplaceOfferFromTemplate(
            [HttpTrigger(AuthorizationLevel.Function, "Post", Route = "offers/{offerId}/template")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateOrUpdateAzureMarketplaceOfferFromTemplate));

                try
                {
                    var requestCotent = await HttpUtils.GetRequestBodyAsync(req);

                    var offer = JsonConvert.DeserializeObject<MarketplaceOffer>(requestCotent, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                    if (offer.OfferId != offerId)
                    {
                        throw new LunaBadRequestUserException(
                            string.Format(ErrorMessages.MARKETPLACE_OFFER_NAME_DOES_NOT_MATCH, offerId, offer.OfferId),
                            UserErrorCode.NameMismatch, target: nameof(offerId));
                    }

                    MarketplaceOfferSnapshotDB snapshot = null;
                    bool isNewOffer = false;

                    var offerEvent = new MarketplaceEventDB()
                    {
                        EventId = Guid.NewGuid(),
                        ResourceName = offerId,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    var offerDb = await _dbContext.MarketplaceOffers.
                        SingleOrDefaultAsync(x => x.OfferId == offerId && 
                        x.Status != MarketplaceOfferStatus.Deleted.ToString());

                    if (offerDb != null)
                    {
                        offerDb.LastUpdatedTime = DateTime.UtcNow;

                        offerEvent.EventType = MarketplaceEventType.UpdateMarketplaceOfferFromTemplate.ToString();
                        offerEvent.EventContent = await _offerEventGenerator.
                            GenerateUpdateMarketplaceOfferFromTemplateEventContentAsync(requestCotent);
                    }
                    else
                    {
                        offerEvent.EventType = MarketplaceEventType.CreateMarketplaceOfferFromTemplate.ToString();
                        offerEvent.EventContent = await _offerEventGenerator.
                            GenerateCreateMarketplaceOfferFromTemplateEventContentAsync(requestCotent);

                        isNewOffer = true;
                        snapshot = await this.CreateMarketplaceOfferSnapshotAsync(offerId,
                            MarketplaceOfferStatus.Draft,
                            offerEvent,
                            "",
                            isNewOffer);
                    }

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.MarketplaceEvents.Add(offerEvent);
                        await _dbContext._SaveChangesAsync();

                        if (isNewOffer)
                        {
                            _dbContext.MarketplaceOffers.Add(
                                new MarketplaceOfferDB(offerId, 
                                offer.Properties.DisplayName,
                                offer.Properties.Description));
                            await _dbContext._SaveChangesAsync();

                            if (snapshot != null)
                            {
                                snapshot.LastAppliedEventId = offerEvent.Id;
                                _dbContext.MarketplaceOfferSnapshots.Add(snapshot);
                                await _dbContext._SaveChangesAsync();
                            }
                            else
                            {
                                throw new LunaServerException($"Snapshot for offer {offerId} does not exist.");
                            }
                        }
                        else
                        {
                            _dbContext.MarketplaceOffers.Update(offerDb);
                            await _dbContext._SaveChangesAsync();
                        }

                        transaction.Commit();
                    }

                    return new OkObjectResult(offer);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateOrUpdateAzureMarketplaceOfferFromTemplate));
                }
            }
        }

        /// <summary>
        /// Create an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req" in="body">
        ///     <see cref="MarketplaceOfferRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceOfferRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer request
        ///         </summary>
        ///     </example>
        ///     Azure Marketplace offer request
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplaceOfferResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceOfferResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace offer response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateAzureMarketplaceOffer")]
        public async Task<IActionResult> CreateAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateAzureMarketplaceOffer));

                try
                {
                    var request = await HttpUtils.DeserializeRequestBodyAsync<MarketplaceOfferRequest>(req);
                    var response = await this._marketplaceFunction.CreateMarketplaceOfferAsync(offerId, request, lunaHeaders);
                    return new OkObjectResult(response);
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
        /// <url>http://localhost:7071/api/offers/{offerId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req" in="body">
        ///     <see cref="MarketplaceOfferRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceOfferRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer request
        ///         </summary>
        ///     </example>
        ///     Azure marketplace offer request
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplaceOfferResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceOfferResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer request
        ///         </summary>
        ///     </example>
        ///     Azure marketplace offer response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateAzureMarketplaceOffer")]
        public async Task<IActionResult> UpdateAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateAzureMarketplaceOffer));

                try
                {
                    var request = await HttpUtils.DeserializeRequestBodyAsync<MarketplaceOfferRequest>(req);
                    var response = await this._marketplaceFunction.UpdateMarketplaceOfferAsync(offerId, request, lunaHeaders);
                    return new OkObjectResult(response);
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
        /// <url>http://localhost:7071/api/offers/{offerId}/publish</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("PublishAzureMarketplaceOffer")]
        public async Task<IActionResult> PublishAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "offers/{offerId}/publish")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.PublishAzureMarketplaceOffer));

                try
                {
                    await this._marketplaceFunction.PublishMarketplaceOfferAsync(offerId, lunaHeaders);
                    return new NoContentResult();
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
        /// <url>http://localhost:7071/api/offers/{offerId}</url>
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
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteAzureMarketplaceOffer));

                try
                {
                    await this._marketplaceFunction.DeleteMarketplaceOfferAsync(offerId, lunaHeaders);
                    return new NoContentResult();
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
        /// <url>http://localhost:7071/api/offers/{offerId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="MarketplaceOfferResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceOfferResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace offer response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetAzureMarketplaceOffer")]
        public async Task<IActionResult> GetAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetAzureMarketplaceOffer));

                try
                {
                    var response = await this._marketplaceFunction.GetMarketplaceOfferAsync(offerId, lunaHeaders);
                    return new OkObjectResult(response);
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
        /// <url>http://localhost:7071/api/offers</url>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="MarketplaceOfferResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceOfferResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace offer response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace offer response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListAzureMarketplaceOffers")]
        public async Task<IActionResult> ListAzureMarketplaceOffers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "offers")] HttpRequest req)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListAzureMarketplaceOffers));

                try
                {
                    var response = await this._marketplaceFunction.ListMarketplaceOffersAsync(lunaHeaders.UserId, lunaHeaders);
                    return new OkObjectResult(response);
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

        #endregion

        #region Marketplace plans

        /// <summary>
        /// Create a plan in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req" in="body">
        ///     <see cref="MarketplacePlanRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlanRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan request
        ///         </summary>
        ///     </example>
        ///     Azure marketplace plan request
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplacePlanResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlanResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace plan response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateMarketplacePlan")]
        public async Task<IActionResult> CreateMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateMarketplacePlan));

                try
                {
                    var request = await HttpUtils.DeserializeRequestBodyAsync<MarketplacePlanRequest>(req);
                    var response = await this._marketplaceFunction.CreateMarketplacePlanAsync(offerId, planId, request, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateMarketplacePlan));
                }
            }
        }

        /// <summary>
        /// Update a plan in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req" in="body">
        ///     <see cref="MarketplacePlanRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlanRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan request
        ///         </summary>
        ///     </example>
        ///     Azure marketplace plan request
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplacePlanResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlanResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace plan response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateMarketplacePlan")]
        public async Task<IActionResult> UpdateMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateMarketplacePlan));

                try
                {
                    var request = await HttpUtils.DeserializeRequestBodyAsync<MarketplacePlanRequest>(req);
                    var response = await this._marketplaceFunction.UpdateMarketplacePlanAsync(offerId, planId, request, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateMarketplacePlan));
                }
            }
        }

        /// <summary>
        /// Delete a plan in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteMarketplacePlan")]
        public async Task<IActionResult> DeleteMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteMarketplacePlan));

                try
                {
                    await this._marketplaceFunction.DeleteMarketplacePlanAsync(offerId, planId, lunaHeaders);
                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteMarketplacePlan));
                }
            }
        }

        /// <summary>
        /// Get a plan in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="MarketplacePlanResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlanResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace plan response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetMarketplacePlan")]
        public async Task<IActionResult> GetMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMarketplacePlan));

                try
                {
                    var response = await this._marketplaceFunction.GetMarketplacePlanAsync(offerId, planId, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetMarketplacePlan));
                }
            }
        }

        /// <summary>
        /// List plans in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/plans</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is<see cref="MarketplacePlanResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlanResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace plan response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListMarketplacePlans")]
        public async Task<IActionResult> ListMarketplacePlans(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "offers/{offerId}/plans")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMarketplacePlans));

                try
                {
                    var response = await this._marketplaceFunction.ListMarketplacePlansAsync(offerId, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListMarketplacePlans));
                }
            }
        }
        #endregion

        #region Marketplace offer parameters

        /// <summary>
        /// Create a parameter in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/parameters/{parameterName}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="parameterName" required="true" cref="string" in="path">The offer parameter name</param>
        /// <param name="req" in="body">
        ///     <see cref="MarketplaceParameterRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceParameterRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace parameter request
        ///         </summary>
        ///     </example>
        ///     Azure marketplace parameter request
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplaceParameterResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceParameterResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace parameter response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace parameter response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateOfferParameter")]
        public async Task<IActionResult> CreateOfferParameter(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "offers/{offerId}/parameters/{parameterName}")] HttpRequest req,
            string offerId,
            string parameterName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateOfferParameter));

                try
                {
                    var request = await HttpUtils.DeserializeRequestBodyAsync<MarketplaceParameterRequest>(req);
                    var response = await this._marketplaceFunction.CreateParameterAsync(offerId, parameterName, request, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateOfferParameter));
                }
            }
        }

        /// <summary>
        /// Update a parameter in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/parameters/{parameterName}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="parameterName" required="true" cref="string" in="path">The offer parameter name</param>
        /// <param name="req" in="body">
        ///     <see cref="MarketplaceParameterRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceParameterRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace parameter request
        ///         </summary>
        ///     </example>
        ///     Azure marketplace parameter request
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplaceParameterResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceParameterResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace parameter response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace parameter response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateOfferParameter")]
        public async Task<IActionResult> UpdateOfferParameter(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "offers/{offerId}/parameters/{parameterName}")] HttpRequest req,
            string offerId,
            string parameterName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateOfferParameter));

                try
                {
                    var request = await HttpUtils.DeserializeRequestBodyAsync<MarketplaceParameterRequest>(req);
                    var response = await this._marketplaceFunction.UpdateParameterAsync(offerId, parameterName, request, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateOfferParameter));
                }
            }
        }

        /// <summary>
        /// Delete a parameter in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/parameters/{parameterName}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="parameterName" required="true" cref="string" in="path">Name of marketplace SaaS offer parameter</param>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteOfferParameter")]
        public async Task<IActionResult> DeleteOfferParameter(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "offers/{offerId}/parameters/{parameterName}")] HttpRequest req,
            string offerId,
            string parameterName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteOfferParameter));

                try
                {
                    await this._marketplaceFunction.DeleteParameterAsync(offerId, parameterName, lunaHeaders);
                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteOfferParameter));
                }
            }
        }

        /// <summary>
        /// Get a parameter in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/parameters/{parameterName}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="parameterName" required="true" cref="string" in="path">Name of marketplace SaaS offer parameter</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="MarketplaceParameterResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceParameterResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace parameter response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace parameter response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetOfferParameter")]
        public async Task<IActionResult> GetOfferParameter(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "offers/{offerId}/parameters/{parameterName}")] HttpRequest req,
            string offerId,
            string parameterName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetOfferParameter));

                try
                {
                    var response = await this._marketplaceFunction.GetParameterAsync(offerId, parameterName, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetOfferParameter));
                }
            }
        }

        /// <summary>
        /// List parameters in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/parameters</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is<see cref="MarketplaceParameterResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceParameterResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace parameter response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace parameter response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListOfferParameters")]
        public async Task<IActionResult> ListOfferParameters(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "offers/{offerId}/parameters")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListOfferParameters));

                try
                {
                    var response = await this._marketplaceFunction.ListParametersAsync(offerId, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListOfferParameters));
                }
            }
        }
        #endregion

        #region Marketplace provisioning steps

        /// <summary>
        /// Create a provisioning step in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/provisioningsteps/{stepName}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="stepName" required="true" cref="string" in="path">Name of marketplace SaaS provisioning step</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseProvisioningStepRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseProvisioningStepRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace provisioning step request
        ///         </summary>
        ///     </example>
        ///     Azure marketplace provisioning step request
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseProvisioningStepResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseProvisioningStepResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace provisioning step response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace provisioning step response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateProvisioningStep")]
        public async Task<IActionResult> CreateProvisioningStep(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "offers/{offerId}/provisioningsteps/{stepName}")] HttpRequest req,
            string offerId,
            string stepName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateProvisioningStep));

                try
                {
                    var request = await HttpUtils.DeserializeRequestBodyAsync<BaseProvisioningStepRequest>(req);
                    var response = await this._marketplaceFunction.CreateProvisioningStepAsync(offerId, stepName, request, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateProvisioningStep));
                }
            }
        }

        /// <summary>
        /// Update a provisioning step in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/provisioningsteps/{stepName}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="stepName" required="true" cref="string" in="path">Name of marketplace SaaS provisioning step</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseProvisioningStepRequest"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseProvisioningStepRequest.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace provisioning step request
        ///         </summary>
        ///     </example>
        ///     Azure marketplace provisioning step request
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseProvisioningStepResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseProvisioningStepResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace provisioning step response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace provisioning step response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateProvisioningStep")]
        public async Task<IActionResult> UpdateProvisioningStep(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "offers/{offerId}/provisioningsteps/{stepName}")] HttpRequest req,
            string offerId,
            string stepName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateProvisioningStep));

                try
                {
                    var request = await HttpUtils.DeserializeRequestBodyAsync<BaseProvisioningStepRequest>(req);
                    var response = await this._marketplaceFunction.UpdateProvisioningStepAsync(offerId, stepName, request, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateProvisioningStep));
                }
            }
        }

        /// <summary>
        /// Delete a provisioning step in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/provisioningsteps/{stepName}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="stepName" required="true" cref="string" in="path">Name of marketplace SaaS provisioning step</param>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteProvisioningStep")]
        public async Task<IActionResult> DeleteProvisioningStep(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "offers/{offerId}/provisioningsteps/{stepName}")] HttpRequest req,
            string offerId,
            string stepName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteProvisioningStep));

                try
                {
                    await this._marketplaceFunction.DeleteProvisioningStepAsync(offerId, stepName, lunaHeaders);
                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteProvisioningStep));
                }
            }
        }

        /// <summary>
        /// Get a provisioning step in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/provisioningsteps/{stepName}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="stepName" required="true" cref="string" in="path">Name of marketplace SaaS provisioning step</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="BaseProvisioningStepResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseProvisioningStepResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace provisioning step response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace provisioning step response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetProvisioningStep")]
        public async Task<IActionResult> GetProvisioningStep(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "offers/{offerId}/provisioningsteps/{stepName}")] HttpRequest req,
            string offerId,
            string stepName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetProvisioningStep));

                try
                {
                    var response = await this._marketplaceFunction.GetProvisioningStepAsync(offerId, stepName, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetProvisioningStep));
                }
            }
        }

        /// <summary>
        /// List provisioning steps in an Azure Marketplace SaaS offer
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/offers/{offerId}/provisioningsteps</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is<see cref="BaseProvisioningStepResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseProvisioningStepResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace provisioning response
        ///         </summary>
        ///     </example>
        ///     Azure marketplace provisioning response
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListProvisioningSteps")]
        public async Task<IActionResult> ListProvisioningSteps(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "offers/{offerId}/provisioningsteps")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListProvisioningSteps));

                try
                {
                    var response = await this._marketplaceFunction.ListProvisioningStepsAsync(offerId, lunaHeaders);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListProvisioningSteps));
                }
            }
        }
        #endregion

        #region azure marketplace subscriptions

        /// <summary>
        /// Get user input parameters for a certain marketplace plan.
        /// This is a public API and can be called anonymously.
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/public/offers/{offerId}/plans/{planId}/parameters</url>
        /// <param name="offerId" required="true" cref="string" in="path">The offer ID</param>
        /// <param name="planId" required="true" cref="string" in="path">The plan ID</param>
        /// <param name="req">Http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="MarketplaceParameterResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceParameterResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of marketplace parameters
        ///         </summary>
        ///     </example>
        ///     Marketplace user input parameters
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetMarketplaceUserInputParameters")]
        public async Task<IActionResult> GetMarketplaceUserInputParameters(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "public/offers/{offerId}/plans/{planId}/parameters")]
            HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMarketplaceUserInputParameters));

                try
                {
                    var parameters = await this._marketplaceFunction.ListInputParametersAsync(offerId, lunaHeaders);

                    return new OkObjectResult(parameters);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetMarketplaceUserInputParameters));
                }
            }
        }

        /// <summary>
        /// Resolve Azure Marketplace subscription from token
        /// This is a public API and can be called anonymously.
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/public/subscriptions/resolveToken</url>
        /// <param name="req" in="body"><see cref="string"/>Token</param>
        /// <response code="200">
        ///     <see cref="MarketplaceSubscriptionResponse"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplaceSubscriptionResponse.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of a marketplace subscription response
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "public/subscriptions/resolvetoken")]
            HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ResolveMarketplaceSubscription));

                try
                {
                    string requestContent = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await this._marketplaceFunction.ResolveMarketplaceSubscriptionAsync(requestContent, lunaHeaders);
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
        /// <url>http://localhost:7071/api/subscriptions/{subscriptionId}</url>
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "Put", Route = "public/subscriptions/{subscriptionId}")]
            HttpRequest req,
            Guid subscriptionId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateMarketplaceSubscription));

                try
                {
                    var request = await HttpUtils.DeserializeRequestBodyAsync<MarketplaceSubscriptionRequest>(req);
                    var result = await this._marketplaceFunction.CreateMarketplaceSubscriptionAsync(subscriptionId, request, lunaHeaders);
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

        /// <summary>
        /// Activate a Azure Marketplace subscription
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/subscriptions/{subscriptionId}/activate</url>
        /// <param name="subscriptionId" required="true" cref="string" in="path">ID of the subscription</param>
        /// <param name="req">http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ActivateMarketplaceSubscription")]
        public async Task<IActionResult> ActivateMarketplaceSubscription(
            [HttpTrigger(AuthorizationLevel.Function, "Post", Route = "subscriptions/{subscriptionId}/activate")]
            HttpRequest req,
            Guid subscriptionId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ActivateMarketplaceSubscription));

                try
                {
                    await this._marketplaceFunction.ActivateMarketplaceSubscriptionAsync(subscriptionId, lunaHeaders);
                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ActivateMarketplaceSubscription));
                }
            }
        }

        /// <summary>
        /// Unsubscribe a Azure Marketplace subscription
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/subscriptions/{subscriptionId}</url>
        /// <param name="subscriptionId" required="true" cref="string" in="path">ID of the subscription</param>
        /// <param name="req">http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UnsubscribeMarketplaceSubscription")]
        public async Task<IActionResult> UnsubscribeMarketplaceSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Delete", Route = "public/subscriptions/{subscriptionId}")]
            HttpRequest req,
            Guid subscriptionId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UnsubscribeMarketplaceSubscription));

                try
                {
                    await this._marketplaceFunction.DeleteMarketplaceSubscriptionAsync(subscriptionId, lunaHeaders);
                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UnsubscribeMarketplaceSubscription));
                }
            }
        }

        /// <summary>
        /// Get Azure Marketplace subscriptions
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/subscriptions/{subscriptionId}</url>
        /// <param name="subscriptionId" required="true" cref="string" in="path">ID of the subscription</param>
        /// <param name="req">request</param>
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "public/subscriptions/{subscriptionId}")]
            HttpRequest req,
            Guid subscriptionId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMarketplaceSubscription));

                try
                {
                    var result = await this._marketplaceFunction.GetMarketplaceSubscriptionAsync(subscriptionId, lunaHeaders);
                    return new OkObjectResult(result);
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
        /// List Azure Marketplace subscriptions
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/subscriptions</url>
        /// <param name="req">request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is<see cref="MarketplaceSubscriptionResponse"/>
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "public/subscriptions")]
            HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMarketplaceSubscriptions));

                try
                {
                    var result = await this._marketplaceFunction.ListMarketplaceSubscriptionsAsync(lunaHeaders);
                    return new OkObjectResult(result);
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
        /// List Azure Marketplace subscriptions with details
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/subscriptions/details</url>
        /// <param name="req">request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is<see cref="MarketplaceSubscriptionResponse"/>
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "public/subscriptiondetails")]
            HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMarketplaceSubscriptionDetails));

                try
                {
                    if (string.IsNullOrEmpty(lunaHeaders.UserId) && req.Query.ContainsKey("ownerId"))
                    {
                        lunaHeaders.UserId = req.Query["ownerId"].ToString();
                    }

                    var result = await this._marketplaceFunction.ListMarketplaceSubscriptionDetailsAsync(lunaHeaders);
                    return new OkObjectResult(result);
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

        #endregion

        #region Marketplace webhook

        /// <summary>
        /// Marketplace webhooks
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/webhook</url>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("AzureMarketplaceWebhook")]
        public async Task<IActionResult> AzureMarketplaceWebhook(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "webhook")] HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AzureMarketplaceWebhook));

                try
                {
                    return new OkResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AzureMarketplaceWebhook));
                }
            }
        }
        #endregion

        #region signin CLI

        [FunctionName("GetAccessToken")]
        public async Task<IActionResult> GetAccessToken(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "public/manage/accessToken")] HttpRequest req)
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "public/manage/deviceCode")] HttpRequest req)
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

        #region Private methods

        private async Task<MarketplaceOfferSnapshotDB> CreateMarketplaceOfferSnapshotAsync(string offerId,
            MarketplaceOfferStatus status,
            MarketplaceEventDB currentEvent = null,
            string tags = "",
            bool isNewOffer = false)
        {
            var snapshot = isNewOffer ? null : _dbContext.MarketplaceOfferSnapshots.
                Where(x => x.OfferId == offerId).
                OrderByDescending(x => x.LastAppliedEventId).FirstOrDefault();

            var events = new List<BaseMarketplaceEvent>();

            if (!isNewOffer)
            {
                events = await _dbContext.MarketplaceEvents.
                    Where(x => x.Id > snapshot.LastAppliedEventId && x.ResourceName == offerId).
                    OrderBy(x => x.Id).
                    Select(x => x.GetEventObject()).
                    ToListAsync();
            }

            if (currentEvent != null)
            {
                events.Add(currentEvent.GetEventObject());
            }

            var newSnapshot = new MarketplaceOfferSnapshotDB()
            {
                SnapshotId = Guid.NewGuid(),
                OfferId = offerId,
                SnapshotContent = await _offerEventProcessor.GetMarketplaceOfferJSONStringAsync(offerId, events, snapshot),
                Status = status.ToString(),
                Tags = "",
                CreatedTime = DateTime.UtcNow,
                DeletedTime = null
            };

            return newSnapshot;
        }

        #endregion
    }
}
