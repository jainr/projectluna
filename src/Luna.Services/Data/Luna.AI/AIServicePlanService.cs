using Luna.Clients.Azure;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luna.Services.Data.Luna.AI
{
    public class AIServicePlanService : IAIServicePlanService
    {
        private readonly ISqlDbContext _context;
        private readonly IAIServiceService _aiServiceService;
        private readonly IPlanService _planService;
        private readonly ILogger<AIServicePlanService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="aiServiceService">The service to be injected.</param>
        /// <param name="planService">The service to be injected.</param>
        /// <param name="logger">The logger.</param>
        public AIServicePlanService(ISqlDbContext sqlDbContext, IAIServiceService aiServiceService, IPlanService planService, ILogger<AIServicePlanService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _aiServiceService = aiServiceService ?? throw new ArgumentNullException(nameof(aiServiceService));
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all AIServicePlans.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <returns>A list of deployments.</returns>
        public async Task<List<AIServicePlan>> GetAllAsync(string aiServiceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AIServicePlan).Name));

            // Get the aiService associated with the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);

            // Get all aiServicePlans with a FK to the aiService
            var aiServicePlans = await _context.AIServicePlans.Where(d => d.AIServiceId.Equals(aiService.Id)).ToListAsync();

            foreach(var aiServicePlan in aiServicePlans)
            {
                aiServicePlan.AIServiceName = aiService.AIServiceName;
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AIServicePlan).Name, aiServicePlans.Count()));

            return aiServicePlans;
        }

        /// <summary>
        /// Gets an AIServicePlan by name.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>The aiServicePlan.</returns>
        public async Task<AIServicePlan> GetAsync(string aiServiceName, string aiServicePlanName)
        {
            // Check that an aiServicePlan with the provided aiServicePlanName exists within the given aiService
            if (!(await ExistsAsync(aiServiceName, aiServicePlanName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(AIServicePlan).Name,
                        aiServicePlanName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(AIServicePlan).Name, aiServicePlanName));


            // Get the aiService associated with the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);

            // Find the aiServicePlan that matches the aiServicePlanName provided
            var aiServicePlan = await _context.AIServicePlans
                .SingleOrDefaultAsync(a => (a.AIServiceId == aiService.Id) && (a.AIServicePlanName == aiServicePlanName));

            aiServicePlan.AIServiceName = aiService.AIServiceName;
            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(AIServicePlan).Name,
                aiServicePlanName,
                JsonSerializer.Serialize(aiServicePlan)));

            return aiServicePlan;
        }

        /// <summary>
        /// Creates an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlan">The AI service plan to create.</param>
        /// <returns>The created AI service plan.</returns>
        public async Task<AIServicePlan> CreateAsync(string aiServiceName, AIServicePlan aiServicePlan)
        {
            if (aiServicePlan is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AIServicePlan).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (await ExistsAsync(aiServiceName, aiServicePlan.AIServicePlanName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(AIServicePlan).Name,
                    aiServicePlan.AIServicePlanName));
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(AIServicePlan).Name, aiServicePlan.AIServicePlanName, payload: JsonSerializer.Serialize(aiServicePlan)));

            // Get the aiService associated with the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);

            // Set the FK to aiService
            aiServicePlan.AIServiceId = aiService.Id;

            // Update the aiServicePlan created time
            aiServicePlan.CreatedTime = DateTime.UtcNow;

            // Update the aiServicePlan last updated time
            aiServicePlan.LastUpdatedTime = aiServicePlan.CreatedTime;

            using (var transaction = await _context.BeginTransactionAsync())
            {
                // Create SaaS plan if SaaS offer is associated with the aiService
                var offer = await _context.Offers.SingleOrDefaultAsync(o => o.AIServiceId == aiService.Id && o.Status != "Deleted");

                if (offer != null)
                {
                    if ((await _context.Plans.Where(p => p.OfferId == offer.Id && p.PlanName == aiServicePlan.AIServicePlanName).CountAsync()) == 0)
                    {
                        Plan plan = new Plan();
                        plan.OfferId = offer.Id;
                        plan.PlanName = aiServicePlan.AIServicePlanName;
                        plan.PriceModel = "flatRate";
                        await _planService.CreateAsync(aiService.SaaSOfferName, plan);
                        await _context._SaveChangesAsync();
                    }
                }

                // Add aiServicePlan to db
                _context.AIServicePlans.Add(aiServicePlan);

                await _context._SaveChangesAsync();
                transaction.Commit();
            }

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(AIServicePlan).Name, aiServicePlan.AIServicePlanName));

            return aiServicePlan;
        }

        /// <summary>
        /// Updates an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to update.</param>
        /// <param name="aiServicePlan">The AI service plan to update.</param>
        /// <returns>The updated AIServicePlan.</returns>
        public async Task<AIServicePlan> UpdateAsync(string aiServiceName, string aiServicePlanName, AIServicePlan aiServicePlan)
        {
            if (aiServicePlan is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AIServicePlan).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if ((aiServicePlanName != aiServicePlan.AIServicePlanName) && (await ExistsAsync(aiServiceName, aiServicePlan.AIServicePlanName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AIServicePlan).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(AIServicePlan).Name, aiServicePlanName, payload: JsonSerializer.Serialize(aiServicePlan)));

            // Get the aiServicePlan that matches the aiServiceName and aiServicePlanName provided
            var aiServicePlanDb = await GetAsync(aiServiceName, aiServicePlanName);

            // Copy over the changes
            aiServicePlanDb.Copy(aiServicePlan);

            // Update the aiServicePlan last updated time
            aiServicePlanDb.LastUpdatedTime = DateTime.UtcNow;

            // Update aiServicePlan values and save changes in db
            _context.AIServicePlans.Update(aiServicePlanDb);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(AIServicePlan).Name, aiServicePlanName));

            return aiServicePlanDb;
        }

        /// <summary>
        /// Deletes an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>The deleted AIServicePlan.</returns>
        public async Task<AIServicePlan> DeleteAsync(string aiServiceName, string aiServicePlanName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(AIServicePlan).Name, aiServicePlanName));

            // Get the aiServicePlan that matches the aiServiceName and aiServicePlanName provided
            var aiServicePlan = await GetAsync(aiServiceName, aiServicePlanName);

            // Remove the aiServicePlan from the db
            _context.AIServicePlans.Remove(aiServicePlan);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(AIServicePlan).Name, aiServicePlanName));

            return aiServicePlan;
        }

        /// <summary>
        /// Checks if an AIServicePlan exists.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string aiServiceName, string aiServicePlanName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(AIServicePlan).Name, aiServicePlanName));

            //Get the aiService associated with the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);

            // Check that only one aiServicePlan with this aiServicePlanName exists within the aiService
            var count = await _context.AIServicePlans
                .CountAsync(d => (d.AIServiceId == aiService.Id) && (d.AIServicePlanName == aiServicePlanName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(AIServicePlan).Name,
                    aiServicePlanName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AIServicePlan).Name, aiServicePlanName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AIServicePlan).Name, aiServicePlanName, true));
                // count = 1
                return true;
            }
        }
    }
}

