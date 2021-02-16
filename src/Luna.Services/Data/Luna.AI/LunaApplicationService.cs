using Luna.Clients;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class LunaApplicationService : ILunaApplicationService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<LunaApplicationService> _logger;
        private readonly IOfferService _offerService;
        private readonly IGatewayService _gatewayService;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public LunaApplicationService(ISqlDbContext sqlDbContext, ILogger<LunaApplicationService> logger, IOfferService offerService, IGatewayService gatewayService)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _gatewayService = gatewayService ?? throw new ArgumentNullException(nameof(gatewayService));
        }

        /// <summary>
        /// Gets all AIServices.
        /// </summary>
        /// <returns>A list of AIServices.</returns>
        public async Task<List<LunaApplication>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(LunaApplication).Name));

            // Get all aiServices
            var aiServices = await _context.LunaApplications.ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(LunaApplication).Name, aiServices.Count()));

            return aiServices;
        }

        /// <summary>
        /// Gets an AIService by name.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to get.</param>
        /// <returns>The AIService.</returns>
        public async Task<LunaApplication> GetAsync(string aiServiceName)
        {
            if (!await ExistsAsync(aiServiceName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(LunaApplication).Name,
                    aiServiceName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(LunaApplication).Name, aiServiceName));

            // Get the aiService that matches the provided aiServiceName
            var aiService = await _context.LunaApplications.SingleOrDefaultAsync(o => (o.ApplicationName == aiServiceName));
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(LunaApplication).Name,
               aiServiceName,
               JsonSerializer.Serialize(aiService)));

            return aiService;
        }

        /// <summary>
        /// Creates an AIService.
        /// </summary>
        /// <param name="aiService">The AIService to create.</param>
        /// <returns>The created AIService.</returns>
        public async Task<LunaApplication> CreateAsync(LunaApplication aiService)
        {
            if (aiService is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(LunaApplication).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that an offer with the same name does not already exist
            if (await ExistsAsync(aiService.ApplicationName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(LunaApplication).Name,
                        aiService.ApplicationName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(LunaApplication).Name, aiService.ApplicationName, payload: JsonSerializer.Serialize(aiService)));

            // Update the aiService created time
            aiService.CreatedTime = DateTime.UtcNow;

            // Update the aiService last updated time
            aiService.LastUpdatedTime = aiService.CreatedTime;

            
            using (var transaction = await _context.BeginTransactionAsync())
            {
                _context.LunaApplications.Add(aiService);
                await _context._SaveChangesAsync();

                // Create SaaS offer and plan if required
                if (!string.IsNullOrEmpty(aiService.SaaSOfferName) && !string.IsNullOrEmpty(aiService.SaaSOfferPlanName))
                {
                    long offerId = -1;
                    long planId = -1;
                    if (!await _offerService.ExistsAsync(aiService.SaaSOfferName))
                    {
                        Offer offer = new Offer()
                        {
                            OfferName = aiService.SaaSOfferName,
                            DisplayName = aiService.SaaSOfferName,
                            Owners = aiService.Owner,
                            HostSubscription = Guid.Empty,
                            Description = aiService.Description,
                            OfferVersion = "v1",
                            IsAzureMarketplaceOffer = false,
                            IsInternalApplication = true
                        };
                        await _offerService.CreateAsync(offer);
                        await _context._SaveChangesAsync();
                        offerId = offer.Id;
                    }
                    else
                    {
                        var offer = await _offerService.GetAsync(aiService.SaaSOfferName);
                        offerId = offer.Id;
                    }

                    if (!await _context.Plans.Where(x => x.OfferId == offerId && x.PlanName == aiService.SaaSOfferPlanName).AnyAsync())
                    {
                        Plan plan = new Plan()
                        {
                            OfferId = offerId,
                            PlanName = aiService.SaaSOfferPlanName,
                            PlanDisplayName = aiService.SaaSOfferPlanName,
                            PriceModel = "flatRate",
                            Description = aiService.Description,
                            ApplicationNames = new List<string>(new string[] { aiService.ApplicationName })
                        };
                        _context.Plans.Add(plan);
                        await _context._SaveChangesAsync();
                        planId = plan.Id;
                    }
                    else
                    {
                        var plan = await _context.Plans.Where(x => x.OfferId == offerId && x.PlanName == aiService.SaaSOfferPlanName).SingleOrDefaultAsync();
                        planId = plan.Id;
                    }

                    _context.PlanApplications.Add(new PlanApplication()
                    {
                        PlanId = planId,
                        ApplicationId = aiService.Id
                    });
                    await _context._SaveChangesAsync();

                    foreach (var gateway in await _gatewayService.GetAllPublicAsync())
                    {
                        await _context.PlanGateways.AddAsync(new PlanGateway()
                        {
                            PlanId = planId,
                            GatewayId = gateway.Id
                        });
                    }
                    await _context._SaveChangesAsync();

                }

                transaction.Commit();
            }

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(LunaApplication).Name, aiService.ApplicationName));

            return aiService;
        }

        /// <summary>
        /// Updates an AIService.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to update.</param>
        /// <param name="aiService">The updated AIService.</param>
        /// <returns>The updated AIService.</returns>
        public async Task<LunaApplication> UpdateAsync(string aiServiceName, LunaApplication aiService)
        {
            if (aiService is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(LunaApplication).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(LunaApplication).Name, aiService.ApplicationName, payload: JsonSerializer.Serialize(aiService)));

            // Get the aiService that matches the offerName provided
            var aiServiceDb = await GetAsync(aiServiceName);

            if ((aiServiceName != aiService.ApplicationName) && (await ExistsAsync(aiService.ApplicationName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(LunaApplication).Name),
                    UserErrorCode.NameMismatch);
            }

            aiServiceDb.Copy(aiService);

            // Update the aiService last updated time
            aiServiceDb.LastUpdatedTime = DateTime.UtcNow;

            // Update productDb values and save changes in db
            _context.LunaApplications.Update(aiServiceDb);
            await _context._SaveChangesAsync();
            
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(LunaApplication).Name, aiService.ApplicationName));

            return aiServiceDb;
        }

        /// <summary>
        /// Deletes an AIService.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to delete.</param>
        /// <returns>The deleted AIService.</returns>
        public async Task<LunaApplication> DeleteAsync(string aiServiceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(LunaApplication).Name, aiServiceName));

            // Get the offer that matches the aiServiceName provide
            var aiService = await GetAsync(aiServiceName);

            // Remove the aiService from the db
            _context.LunaApplications.Remove(aiService);
            await _context._SaveChangesAsync();
            
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(LunaApplication).Name, aiServiceName));

            return aiService;
        }

        /// <summary>
        /// Checks if an AIService exists.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string aiServiceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(LunaApplication).Name, aiServiceName));

            // Check that only one offer with this aiServiceName exists and has not been deleted
            var count = await _context.LunaApplications
                .CountAsync(p => (p.ApplicationName == aiServiceName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(LunaApplication).Name, aiServiceName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(LunaApplication).Name, aiServiceName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(LunaApplication).Name, aiServiceName, true));
                // count = 1
                return true;
            }
        }
    }
}
