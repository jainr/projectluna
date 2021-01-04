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
    public class AIServiceService : IAIServiceService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<AIServiceService> _logger;
        private readonly IOfferService _offerService;
        private readonly IWebhookService _webhookService;
        private readonly LunaClient _lunaClient;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        public AIServiceService(ISqlDbContext sqlDbContext, ILogger<AIServiceService> logger, IOfferService offerService, IWebhookService webhookService, LunaClient lunaClient)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
            _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
            _lunaClient = lunaClient ?? throw new ArgumentNullException(nameof(lunaClient));
        }

        /// <summary>
        /// Gets all AIServices.
        /// </summary>
        /// <returns>A list of AIServices.</returns>
        public async Task<List<AIService>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AIService).Name));

            // Get all aiServices
            var aiServices = await _context.AIServices.ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AIService).Name, aiServices.Count()));

            return aiServices;
        }

        /// <summary>
        /// Gets an AIService by name.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to get.</param>
        /// <returns>The AIService.</returns>
        public async Task<AIService> GetAsync(string aiServiceName)
        {
            if (!await ExistsAsync(aiServiceName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(AIService).Name,
                    aiServiceName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(AIService).Name, aiServiceName));

            // Get the aiService that matches the provided aiServiceName
            var aiService = await _context.AIServices.SingleOrDefaultAsync(o => (o.AIServiceName == aiServiceName));
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(AIService).Name,
               aiServiceName,
               JsonSerializer.Serialize(aiService)));

            return aiService;
        }

        /// <summary>
        /// Gets a aiService by offer id
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <returns></returns>
        public async Task<AIService> GetByOfferIdAsync(long offerId)
        {
            _logger.LogInformation($"Get aiService by SaaS offer id {offerId}");
            var offer = await _context.Offers.FindAsync(offerId);

            if (offer != null && offer.AIServiceId.HasValue)
            {
                var aiService = await _context.AIServices.FindAsync(offer.AIServiceId);

                _logger.LogInformation($"Return aiService by SaaS offer id {offerId} with aiService name {aiService.AIServiceName}.");
                return aiService;
            }
            else
            {
                _logger.LogInformation($"Couldn't find aiService by SaaS offer id {offerId}.");
                return null;
            }
        }

        /// <summary>
        /// Creates an AIService.
        /// </summary>
        /// <param name="aiService">The AIService to create.</param>
        /// <returns>The created AIService.</returns>
        public async Task<AIService> CreateAsync(AIService aiService)
        {
            if (aiService is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AIService).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that an offer with the same name does not already exist
            if (await ExistsAsync(aiService.AIServiceName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(AIService).Name,
                        aiService.AIServiceName));
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(AIService).Name, aiService.AIServiceName, payload: JsonSerializer.Serialize(aiService)));

            // Update the aiService created time
            aiService.CreatedTime = DateTime.UtcNow;

            // Update the aiService last updated time
            aiService.LastUpdatedTime = aiService.CreatedTime;

            // Create SaaS offer with same name if SaaS offer name is not specified
            if (string.IsNullOrEmpty(aiService.SaaSOfferName))
            {
                aiService.SaaSOfferName = aiService.AIServiceName;
            }
            
            using (var transaction = await _context.BeginTransactionAsync())
            {
                _context.AIServices.Add(aiService);
                await _context._SaveChangesAsync();

                Offer offer = new Offer();
                offer.OfferName = aiService.SaaSOfferName;
                offer.OfferAlias = aiService.SaaSOfferName;
                offer.Owners = aiService.Owner;
                offer.HostSubscription = Guid.Empty;
                offer.OfferVersion = "v1";
                offer.AIServiceId = aiService.Id;
                await _offerService.CreateAsync(offer);
                await _context._SaveChangesAsync();

                transaction.Commit();
            }

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(AIService).Name, aiService.AIServiceName));

            return aiService;
        }

        /// <summary>
        /// Updates an AIService.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to update.</param>
        /// <param name="aiService">The updated AIService.</param>
        /// <returns>The updated AIService.</returns>
        public async Task<AIService> UpdateAsync(string aiServiceName, AIService aiService)
        {
            if (aiService is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AIService).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(AIService).Name, aiService.AIServiceName, payload: JsonSerializer.Serialize(aiService)));

            // Get the aiService that matches the offerName provided
            var aiServiceDb = await GetAsync(aiServiceName);

            if ((aiServiceName != aiService.AIServiceName) && (await ExistsAsync(aiService.AIServiceName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AIService).Name),
                    UserErrorCode.NameMismatch);
            }

            aiServiceDb.Copy(aiService);

            // Update the aiService last updated time
            aiServiceDb.LastUpdatedTime = DateTime.UtcNow;

            // Update productDb values and save changes in db
            _context.AIServices.Update(aiServiceDb);
            await _context._SaveChangesAsync();
            
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(AIService).Name, aiService.AIServiceName));

            return aiServiceDb;
        }

        /// <summary>
        /// Deletes an AIService.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to delete.</param>
        /// <returns>The deleted AIService.</returns>
        public async Task<AIService> DeleteAsync(string aiServiceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(AIService).Name, aiServiceName));

            // Get the offer that matches the aiServiceName provide
            var aiService = await GetAsync(aiServiceName);

            // Remove the aiService from the db
            _context.AIServices.Remove(aiService);
            await _context._SaveChangesAsync();
            
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(AIService).Name, aiServiceName));

            return aiService;
        }

        /// <summary>
        /// Checks if an AIService exists.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string aiServiceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(AIService).Name, aiServiceName));

            // Check that only one offer with this aiServiceName exists and has not been deleted
            var count = await _context.AIServices
                .CountAsync(p => (p.AIServiceName == aiServiceName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(AIService).Name, aiServiceName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AIService).Name, aiServiceName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AIService).Name, aiServiceName, true));
                // count = 1
                return true;
            }
        }
    }
}
