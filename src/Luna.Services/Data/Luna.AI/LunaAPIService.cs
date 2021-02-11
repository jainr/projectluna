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
    public class LunaAPIService : ILunaAPIService
    {
        private readonly ISqlDbContext _context;
        private readonly ILunaApplicationService _aiServiceService;
        private readonly IPlanService _planService;
        private readonly ILogger<LunaAPIService> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="aiServiceService">The service to be injected.</param>
        /// <param name="planService">The service to be injected.</param>
        /// <param name="gatewayService">The service to be injected.</param>
        /// <param name="logger">The logger.</param>
        public LunaAPIService(ISqlDbContext sqlDbContext, ILunaApplicationService aiServiceService, 
            IPlanService planService, ILogger<LunaAPIService> logger)
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
        public async Task<List<LunaAPI>> GetAllAsync(string aiServiceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(LunaAPI).Name));

            // Get the aiService associated with the aiServiceName provided
            var application = await _aiServiceService.GetAsync(aiServiceName);

            // Get all aiServicePlans with a FK to the aiService
            var aiServicePlans = await _context.LunaAPIs.Where(d => d.ApplicationId.Equals(application.Id)).ToListAsync();

            foreach(var aiServicePlan in aiServicePlans)
            {
                aiServicePlan.ApplicationName = application.ApplicationName;
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(LunaAPI).Name, aiServicePlans.Count()));

            return aiServicePlans;
        }

        /// <summary>
        /// Gets an AIServicePlan by name.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>The aiServicePlan.</returns>
        public async Task<LunaAPI> GetAsync(string aiServiceName, string aiServicePlanName)
        {
            // Check that an aiServicePlan with the provided aiServicePlanName exists within the given aiService
            if (!(await ExistsAsync(aiServiceName, aiServicePlanName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(LunaAPI).Name,
                        aiServicePlanName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(LunaAPI).Name, aiServicePlanName));


            // Get the aiService associated with the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);

            // Find the aiServicePlan that matches the aiServicePlanName provided
            var aiServicePlan = await _context.LunaAPIs
                .SingleOrDefaultAsync(a => (a.ApplicationId == aiService.Id) && (a.APIName == aiServicePlanName));

            aiServicePlan.ApplicationName = aiService.ApplicationName;

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(LunaAPI).Name,
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
        public async Task<LunaAPI> CreateAsync(string aiServiceName, LunaAPI aiServicePlan)
        {
            if (aiServicePlan is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(LunaAPI).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (await ExistsAsync(aiServiceName, aiServicePlan.APIName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(LunaAPI).Name,
                    aiServicePlan.APIName));
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(LunaAPI).Name, aiServicePlan.APIName, payload: JsonSerializer.Serialize(aiServicePlan)));

            // Get the aiService associated with the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);

            // Set the FK to aiService
            aiServicePlan.ApplicationId = aiService.Id;

            // Update the aiServicePlan created time
            aiServicePlan.CreatedTime = DateTime.UtcNow;

            // Update the aiServicePlan last updated time
            aiServicePlan.LastUpdatedTime = aiServicePlan.CreatedTime;

            // Add aiServicePlan to db
            _context.LunaAPIs.Add(aiServicePlan);

            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(LunaAPI).Name, aiServicePlan.APIName));

            return aiServicePlan;
        }

        /// <summary>
        /// Updates an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to update.</param>
        /// <param name="aiServicePlan">The AI service plan to update.</param>
        /// <returns>The updated AIServicePlan.</returns>
        public async Task<LunaAPI> UpdateAsync(string aiServiceName, string aiServicePlanName, LunaAPI aiServicePlan)
        {
            if (aiServicePlan is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(LunaAPI).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if ((aiServicePlanName != aiServicePlan.APIName) && (await ExistsAsync(aiServiceName, aiServicePlan.APIName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(LunaAPI).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(LunaAPI).Name, aiServicePlanName, payload: JsonSerializer.Serialize(aiServicePlan)));

            // Get the aiServicePlan that matches the aiServiceName and aiServicePlanName provided
            var aiServicePlanDb = await GetAsync(aiServiceName, aiServicePlanName);

            // Copy over the changes
            aiServicePlanDb.Copy(aiServicePlan);

            // Update the aiServicePlan last updated time
            aiServicePlanDb.LastUpdatedTime = DateTime.UtcNow;

            // Update aiServicePlan values and save changes in db
            _context.LunaAPIs.Update(aiServicePlanDb);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(LunaAPI).Name, aiServicePlanName));

            return aiServicePlanDb;
        }

        /// <summary>
        /// Deletes an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>The deleted AIServicePlan.</returns>
        public async Task<LunaAPI> DeleteAsync(string aiServiceName, string aiServicePlanName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(LunaAPI).Name, aiServicePlanName));

            // Get the aiServicePlan that matches the aiServiceName and aiServicePlanName provided
            var aiServicePlan = await GetAsync(aiServiceName, aiServicePlanName);

            // Remove the aiServicePlan from the db
            _context.LunaAPIs.Remove(aiServicePlan);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(LunaAPI).Name, aiServicePlanName));

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
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(LunaAPI).Name, aiServicePlanName));

            //Get the aiService associated with the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);

            // Check that only one aiServicePlan with this aiServicePlanName exists within the aiService
            var count = await _context.LunaAPIs
                .CountAsync(d => (d.ApplicationId == aiService.Id) && (d.APIName == aiServicePlanName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(LunaAPI).Name,
                    aiServicePlanName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(LunaAPI).Name, aiServicePlanName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(LunaAPI).Name, aiServicePlanName, true));
                // count = 1
                return true;
            }
        }
    }
}

