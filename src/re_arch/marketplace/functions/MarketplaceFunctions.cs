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
        private readonly ILogger<MarketplaceFunctions> _logger;

        public MarketplaceFunctions(IOfferEventProcessor offerEventProcessor,
            IOfferEventContentGenerator offerEventGenerator,
            ISqlDbContext dbContext,
            IAzureKeyVaultUtils keyVaultUtils,
            IPubSubServiceClient pubSubClient,
            IAzureMarketplaceSaaSClient marketplaceClient,
            ILogger<MarketplaceFunctions> logger)
        {
            this._offerEventGenerator = offerEventGenerator ?? throw new ArgumentNullException(nameof(offerEventGenerator));
            this._offerEventProcessor = offerEventProcessor ?? throw new ArgumentNullException(nameof(offerEventProcessor));
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
            this._pubSubClient = pubSubClient ?? throw new ArgumentNullException(nameof(pubSubClient));
            this._marketplaceClient = marketplaceClient ?? throw new ArgumentNullException(nameof(marketplaceClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Azure Marketplace SaaS offers

        /// <summary>
        /// Create or update an Azure Marketplace SaaS offer from template
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/template</url>
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
            [HttpTrigger(AuthorizationLevel.Function, "Post", Route = "marketplace/offers/{offerId}/template")] HttpRequest req,
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
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "marketplace/offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateAzureMarketplaceOffer));

                try
                {
                    return new BadRequestResult();
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
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "marketplace/offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateAzureMarketplaceOffer));

                try
                {
                    return new BadRequestResult();
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
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("PublishAzureMarketplaceOffer")]
        public async Task<IActionResult> PublishAzureMarketplaceOffer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "marketplace/offers/{offerId}/publish")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.PublishAzureMarketplaceOffer));

                try
                {
                    var offerDb = await _dbContext.MarketplaceOffers.
                        SingleOrDefaultAsync(x => x.OfferId == offerId && 
                        x.Status != MarketplaceOfferStatus.Deleted.ToString());

                    if (offerDb == null)
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.MARKETPLACE_OFFER_DOES_NOT_EXIST, offerId));
                    }

                    offerDb.Publish();

                    var snapshotDb = await this.CreateMarketplaceOfferSnapshotAsync(offerId, MarketplaceOfferStatus.Published);

                    var publishEvent = new PublishAzureMarketplaceOfferEventEntity(offerId, snapshotDb.SnapshotContent);

                    var comments = req.Query.ContainsKey("comments") ? req.Query["comments"].ToString() : "";

                    var offerEvent = new PublishMarketplaceOfferEvent()
                    {
                        Comments = comments,
                    };

                    var eventContent = JsonConvert.SerializeObject(offerEvent, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

                    var offerEventDb = new MarketplaceEventDB(
                        offerId,
                        MarketplaceEventType.PublishMarketplaceOffer.ToString(),
                        eventContent,
                        lunaHeaders.UserId,
                        "");

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.MarketplaceOffers.Update(offerDb);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.MarketplaceEvents.Add(offerEventDb);
                        await _dbContext._SaveChangesAsync();

                        snapshotDb.LastAppliedEventId = offerEventDb.Id;
                        _dbContext.MarketplaceOfferSnapshots.Add(snapshotDb);
                        await _dbContext._SaveChangesAsync();

                        await _pubSubClient.PublishEventAsync(
                            LunaEventStoreType.AZURE_MARKETPLACE_OFFER_EVENT_STORE, 
                            publishEvent, 
                            lunaHeaders);

                        transaction.Commit();
                    }

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
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "marketplace/offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteAzureMarketplaceOffer));

                try
                {
                    var offerDb = await _dbContext.MarketplaceOffers.SingleOrDefaultAsync(
                        x => x.Status != MarketplaceOfferStatus.Deleted.ToString() &&
                        x.OfferId == offerId);

                    if (offerDb == null)
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.MARKETPLACE_OFFER_DOES_NOT_EXIST, offerId));
                    }

                    offerDb.Delete();

                    var deleteEvent = new DeleteAzureMarketplaceOfferEventEntity(offerId);

                    var offerEvent = new DeleteMarketplaceOfferEvent()
                    {
                    };

                    var eventContent = JsonConvert.SerializeObject(offerEvent, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

                    var offerEventDb = new MarketplaceEventDB(
                        offerId,
                        MarketplaceEventType.DeleteMarketplaceOffer.ToString(),
                        eventContent,
                        lunaHeaders.UserId,
                        "");

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.MarketplaceOffers.Update(offerDb);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.MarketplaceEvents.Add(offerEventDb);
                        await _dbContext._SaveChangesAsync();

                        await _pubSubClient.PublishEventAsync(
                            LunaEventStoreType.AZURE_MARKETPLACE_OFFER_EVENT_STORE,
                            deleteEvent,
                            lunaHeaders);

                        transaction.Commit();
                    }

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
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "marketplace/offers/{offerId}")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetAzureMarketplaceOffer));

                try
                {
                    var offerDb = await _dbContext.MarketplaceOffers.
                        SingleOrDefaultAsync(
                            x => x.Status != MarketplaceOfferStatus.Deleted.ToString() &&
                            x.OfferId == offerId);

                    if (offerDb == null)
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.MARKETPLACE_OFFER_DOES_NOT_EXIST, offerId));
                    }

                    var snapshot = await _dbContext.MarketplaceOfferSnapshots.
                        Where(x => x.OfferId == offerId).
                        OrderByDescending(x => x.LastAppliedEventId).FirstOrDefaultAsync();

                    var events = await _dbContext.MarketplaceEvents.
                        Where(x => x.Id > snapshot.LastAppliedEventId && x.ResourceName == offerId).
                        OrderBy(x => x.Id).
                        Select(x => x.GetEventObject()).
                        ToListAsync();

                    var offer = _offerEventProcessor.GetMarketplaceOffer(offerId, events, snapshot);

                    if (!string.IsNullOrEmpty(offer.ProvisioningStepsSecretName))
                    {
                        var content = await this._keyVaultUtils.GetSecretAsync(offer.ProvisioningStepsSecretName);
                        var steps = JsonConvert.DeserializeObject<List<MarketplaceProvisioningStep>>(content, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.All
                        });
                        offer.ProvisioningSteps = steps;
                        offer.ProvisioningStepsSecretName = null;
                    }

                    return new OkObjectResult(offer);
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
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "marketplace/offers")] HttpRequest req)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListAzureMarketplaceOffers));

                try
                {
                    var offers = await _dbContext.MarketplaceOffers.Where(
                        x => x.Status != MarketplaceOfferStatus.Deleted.ToString()).
                        Select(x => x.ToMarketplaceOffer()).
                        ToListAsync();

                    return new OkObjectResult(offers);
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
        ///     <see cref="MarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlan.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlan.example"/>
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
        [FunctionName("CreateMarketplacePlan")]
        public async Task<IActionResult> CreateMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "marketplace/offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateMarketplacePlan));

                try
                {

                    return new BadRequestResult();
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
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req" in="body">
        ///     <see cref="MarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlan.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure marketplace plan
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="MarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlan.example"/>
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
        [FunctionName("UpdateMarketplacePlan")]
        public async Task<IActionResult> UpdateMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "marketplace/offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateMarketplacePlan));

                try
                {
                    return new BadRequestResult();
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
        [FunctionName("DeleteMarketplacePlan")]
        public async Task<IActionResult> DeleteMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "marketplace/offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteMarketplacePlan));

                try
                {
                    return new BadRequestResult();
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
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/plans/{planId}</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="planId" required="true" cref="string" in="path">Id of marketplace SaaS plan</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="MarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlan.example"/>
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
        [FunctionName("GetMarketplacePlan")]
        public async Task<IActionResult> GetMarketplacePlan(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "marketplace/offers/{offerId}/plans/{planId}")] HttpRequest req,
            string offerId,
            string planId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMarketplacePlan));

                try
                {
                    return new BadRequestResult();
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
        /// <url>http://localhost:7071/api/marketplace/offers/{offerId}/plans</url>
        /// <param name="offerId" required="true" cref="string" in="path">Id of marketplace SaaS offer</param>
        /// <param name="req">http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is<see cref="MarketplacePlan"/>
        ///     <example>
        ///         <value>
        ///             <see cref="MarketplacePlan.example"/>
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
        [FunctionName("ListMarketplacePlans")]
        public async Task<IActionResult> ListMarketplacePlans(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "marketplace/offers/{offerId}/plans")] HttpRequest req,
            string offerId)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMarketplacePlans));

                try
                {
                    return new BadRequestResult();
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


        #region azure marketplace subscriptions

        /// <summary>
        /// Get parameters
        /// </summary>
        /// <group>Azure Marketplace</group>
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
                    var plan = await _dbContext.PublishedAzureMarketplacePlans.
                        SingleOrDefaultAsync(x => x.IsEnabled && x.MarketplaceOfferId == offerId && x.MarketplacePlanId == planId);

                    if (plan == null)
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.MARKETPLACE_PLAN_DOES_NOT_EXIST, planId, offerId));
                    }

                    var parameters = JsonConvert.
                        DeserializeObject<List<MarketplaceParameter>>(plan.Parameters, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        });

                    return new OkObjectResult(parameters);
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
                    string requestContent = await HttpUtils.GetRequestBodyAsync(req);
                    var result = await _marketplaceClient.ResolveMarketplaceSubscriptionAsync(requestContent, lunaHeaders);
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
                    var subscription = await HttpUtils.DeserializeRequestBodyAsync<MarketplaceSubscription>(req);

                    if (subscription.Id != subscriptionId)
                    {
                        throw new LunaBadRequestUserException(
                            string.Format(ErrorMessages.MARKETPLACE_SUB_ID_DOES_NOT_MATCH, subscriptionId, subscription.Id),
                            UserErrorCode.NameMismatch);
                    }

                    if (await _dbContext.AzureMarketplaceSubscriptions.AnyAsync(x => x.SubscriptionId == subscriptionId))
                    {
                        throw new LunaConflictUserException(
                            string.Format(ErrorMessages.MARKETPLACE_SUBSCIRPTION_ALREADY_EXIST, subscriptionId));
                    }

                    if (string.IsNullOrEmpty(subscription.Token))
                    {
                        throw new LunaBadRequestUserException(ErrorMessages.INVALID_MARKETPLACE_TOKEN, UserErrorCode.InvalidToken);
                    }

                    var result = await _marketplaceClient.ResolveMarketplaceSubscriptionAsync(subscription.Token, lunaHeaders);

                    // Validate the token avoid people creating subscriptions randomly
                    if (!result.PlanId.Equals(subscription.PlanId) ||
                        !result.OfferId.Equals(subscription.OfferId) ||
                        !result.Id.Equals(subscription.Id))
                    {
                        throw new LunaBadRequestUserException(ErrorMessages.INVALID_MARKETPLACE_TOKEN, UserErrorCode.InvalidToken);
                    }

                    var plan = await _dbContext.PublishedAzureMarketplacePlans.SingleOrDefaultAsync(x => x.IsEnabled &&
                        x.MarketplaceOfferId == subscription.OfferId && x.MarketplacePlanId == subscription.PlanId);

                    if (plan == null)
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.MARKETPLACE_PLAN_DOES_NOT_EXIST, subscription.PlanId, subscription.OfferId));
                    }

                    var requiredParameters = JsonConvert.
                        DeserializeObject<List<MarketplaceParameter>>(plan.Parameters, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        });

                    foreach (var param in requiredParameters)
                    {
                        if (param.IsRequired && !subscription.InputParameters.Any(x => x.Name == param.ParameterName))
                        {
                            throw new LunaBadRequestUserException(
                                string.Format(ErrorMessages.REQUIRED_PARAMETER_NOT_PROVIDED, param.ParameterName),
                                UserErrorCode.ParameterNotProvided,
                                target: param.ParameterName);
                        }
                    }

                    if (plan.Mode == MarketplacePlanMode.IaaS.ToString())
                    {
                        if (!IaaSParameterConstants.VerifyIaaSParameters(subscription.InputParameters.Select(x => x.Name).ToList()))
                        {
                            throw new LunaBadRequestUserException(
                                string.Format(ErrorMessages.REQUIRED_PARAMETER_NOT_PROVIDED, "IaaS"),
                                UserErrorCode.ParameterNotProvided);
                        }
                        CopyParameter(subscription, IaaSParameterConstants.REGION_PARAM_NAME, JumpboxParameterConstants.JUMPBOX_VM_LOCATION_PARAM_NAME);
                        CopyParameter(subscription, IaaSParameterConstants.SUBSCRIPTION_ID_PARAM_NAME, JumpboxParameterConstants.JUMPBOX_VM_SUB_ID_PARAM_NAME);
                        CopyParameter(subscription, IaaSParameterConstants.RESOURCE_GROUP_PARAM_NAME, JumpboxParameterConstants.JUMPBOX_VM_RG_PARAM_NAME);
                        subscription.InputParameters.Add(new MarketplaceSubscriptionParameter
                        {
                            Name = JumpboxParameterConstants.JUMPBOX_VM_NAME_PARAM_NAME,
                            Value = Guid.NewGuid().ToString(),
                            IsSystemParameter = true,
                            Type = MarketplaceParameterValueType.String.ToString()
                        });
                    }

                    var subDb = new AzureMarketplaceSubscriptionDB(subscription, lunaHeaders.UserId, plan.CreatedByEventId);

                    subDb.ParameterSecretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.MARKETPLACE_SUBCRIPTION_PARAMETERS);

                    var paramContent = JsonConvert.SerializeObject(subscription.InputParameters);
                    await _keyVaultUtils.SetSecretAsync(subDb.ParameterSecretName, paramContent);

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.AzureMarketplaceSubscriptions.Add(subDb);
                        await _dbContext._SaveChangesAsync();

                        await _pubSubClient.PublishEventAsync(
                            LunaEventStoreType.AZURE_MARKETPLACE_SUB_EVENT_STORE,
                            new CreateAzureMarketplaceSubscriptionEventEntity(subscriptionId,
                            JsonConvert.SerializeObject(subDb.ToEventContent(), new JsonSerializerSettings()
                            {
                                TypeNameHandling = TypeNameHandling.All
                            })),
                            lunaHeaders);

                        transaction.Commit();
                    }

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

        private void CopyParameter(MarketplaceSubscription subscription, string copyFrom, string newParamName)
        {
            var param = subscription.InputParameters.SingleOrDefault(x => x.Name == copyFrom);
            if (param != null)
            {
                subscription.InputParameters.Add(param.Copy(newParamName));
            }
        }

        /// <summary>
        /// Activate a Azure Marketplace subscription
        /// </summary>
        /// <group>Azure Marketplace</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/marketplace/subscriptions/{subscriptionId}/activate</url>
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "marketplace/subscriptions/{subscriptionId}/activate")]
            HttpRequest req,
            Guid subscriptionId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ActivateMarketplaceSubscription));

                try
                {
                    var subDb = await _dbContext.AzureMarketplaceSubscriptions.
                        SingleOrDefaultAsync(x => x.SubscriptionId == subscriptionId);

                    if (subDb == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.SUBSCIRPTION_DOES_NOT_EXIST, subscriptionId));
                    }

                    if (!subDb.SaaSSubscriptionStatus.Equals(MarketplaceSubscriptionStatus.PENDING_FULFILLMENT_START))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.MARKETPLACE_SUBSCRIPTION_CAN_NOT_BE_ACTIVATED,
                            subscriptionId, subDb.SaaSSubscriptionStatus));
                    }

                    await _marketplaceClient.ActivateMarketplaceSubscriptionAsync(subscriptionId, subDb.PlanId, lunaHeaders);

                    subDb.ActivatedTime = DateTime.UtcNow;
                    subDb.SaaSSubscriptionStatus = MarketplaceSubscriptionStatus.SUBSCRIBED;
                    _dbContext.AzureMarketplaceSubscriptions.Update(subDb);
                    await _dbContext._SaveChangesAsync();

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
        /// <url>http://localhost:7071/api/marketplace/subscriptions/{subscriptionId}</url>
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "Delete", Route = "marketplace/subscriptions/{subscriptionId}")]
            HttpRequest req,
            Guid subscriptionId)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UnsubscribeMarketplaceSubscription));

                try
                {
                    var subDb = await _dbContext.AzureMarketplaceSubscriptions.
                        SingleOrDefaultAsync(x => x.SubscriptionId == subscriptionId);

                    if (subDb == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.SUBSCIRPTION_DOES_NOT_EXIST, subscriptionId));
                    }

                    if (!subDb.SaaSSubscriptionStatus.Equals(MarketplaceSubscriptionStatus.SUBSCRIBED))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.MARKETPLACE_SUBSCRIPTION_CAN_NOT_BE_ACTIVATED,
                            subscriptionId, subDb.SaaSSubscriptionStatus));
                    }

                    await _marketplaceClient.UnsubscribeMarketplaceSubscriptionAsync(subscriptionId, lunaHeaders);

                    subDb.UnsubscribedTime = DateTime.UtcNow;
                    subDb.SaaSSubscriptionStatus = MarketplaceSubscriptionStatus.UNSUBSCRIBED;

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.AzureMarketplaceSubscriptions.Update(subDb);
                        await _dbContext._SaveChangesAsync();

                        await _pubSubClient.PublishEventAsync(
                            LunaEventStoreType.AZURE_MARKETPLACE_OFFER_EVENT_STORE,
                            new DeleteAzureMarketplaceSubscriptionEventEntity(subscriptionId,
                            JsonConvert.SerializeObject(subDb)),
                            lunaHeaders);

                        transaction.Commit();
                    }

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
        /// <url>http://localhost:7071/api/marketplace/subscriptions/{subscriptionId}</url>
        /// <param name="subscriptionId" required="true" cref="string" in="path">ID of the subscription</param>
        /// <param name="req">request</param>
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
                    var subscriptionDb = await _dbContext.AzureMarketplaceSubscriptions.
                        SingleOrDefaultAsync(x => x.SubscriptionId == subscriptionId &&
                        x.SaaSSubscriptionStatus != MarketplaceSubscriptionStatus.UNSUBSCRIBED &&
                        x.OwnerId == lunaHeaders.UserId);

                    if (subscriptionDb == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.SUBSCIRPTION_DOES_NOT_EXIST, subscriptionId));
                    }

                    var result = subscriptionDb.ToMarketplaceSubscription();

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
        /// <url>http://localhost:7071/api/marketplace/subscriptions</url>
        /// <param name="req">request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is<see cref="MarketplaceSubscription"/>
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
        [FunctionName("ListMarketplaceSubscription")]
        public async Task<IActionResult> ListMarketplaceSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "marketplace/subscriptions")]
            HttpRequest req)
        {
            var lunaHeaders = new LunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListMarketplaceSubscription));

                try
                {
                    var subscriptions = await _dbContext.AzureMarketplaceSubscriptions.
                        Where(x => x.OwnerId == lunaHeaders.UserId &&
                        x.SaaSSubscriptionStatus != MarketplaceSubscriptionStatus.UNSUBSCRIBED).
                        Select(x => x.ToMarketplaceSubscription()).
                        ToListAsync();

                    return new OkObjectResult(subscriptions);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListMarketplaceSubscription));
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
                SnapshotContent = _offerEventProcessor.GetMarketplaceOfferJSONString(offerId, events, snapshot),
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
