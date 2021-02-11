using System;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for aiServicePlan resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class AIServicePlanController : ControllerBase
    {
        private readonly ILunaAPIService _aIServicePlanService;
        private readonly ILogger<RestrictedUserController> _logger;
        private readonly IAPIVersionService _apiVersionService;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="deploymentService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public AIServicePlanController(ILunaAPIService aiServicePlanService, ILogger<RestrictedUserController> logger, 
            IAPIVersionService apiVersionService)
        {
            _aIServicePlanService = aiServicePlanService ?? throw new ArgumentNullException(nameof(aiServicePlanService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiVersionService = apiVersionService ?? throw new ArgumentNullException(nameof(apiVersionService));
        }

        /// <summary>
        /// Gets all plans within an aiService.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <returns>HTTP 200 OK with aiServicePlan JSON objects in response body.</returns>
        [HttpGet("aiservices/{aiServiceName}/plans")]
        [HttpGet("applications/{aiServiceName}/apis")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string aiServiceName)
        {
            // all users can call this API.
            _logger.LogInformation($"Get all plans in AI service {aiServiceName}.");
            return Ok(await _aIServicePlanService.GetAllAsync(aiServiceName));
        }

        /// <summary>
        /// Gets a plan within an AI service.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the plan to get.</param>
        /// <returns>HTTP 200 OK with aiServicePlan JSON object in response body.</returns>
        [HttpGet("aiservices/{aiServiceName}/plans/{aiServicePlanName}", Name = nameof(GetAsync) + nameof(LunaAPI))]
        [HttpGet("applications/{aiServiceName}/apis/{aiServicePlanName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string aiServiceName, string aiServicePlanName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get plan {aiServicePlanName} in AI service {aiServiceName}.");
            return Ok(await _aIServicePlanService.GetAsync(aiServiceName, aiServicePlanName));
        }

        /// <summary>
        /// Create or update a aiServicePlan within an aiService.
        /// </summary>
        /// <param name="aiServiceName">The name of the aiService.</param>
        /// <param name="aiServicePlanName">The name of the aiServicePlan to update.</param>
        /// <param name="aiServicePlan">The updated aiServicePlan object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("aiservices/{aiServiceName}/plans/{aiServicePlanName}")]
        [HttpPut("applications/{aiServiceName}/apis/{aiServicePlanName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateOrUpdateAsync(string aiServiceName, string aiServicePlanName, [FromBody] LunaAPI aiServicePlan)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (aiServicePlan == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(aiServicePlan)), UserErrorCode.PayloadNotProvided);
            }

            if (!aiServicePlanName.Equals(aiServicePlan.APIName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(LunaAPI).Name),
                    UserErrorCode.NameMismatch);
            }
            if (await _aIServicePlanService.ExistsAsync(aiServiceName, aiServicePlanName))
            {
                _logger.LogInformation($"Update aiServicePlan {aiServicePlanName} in aiService {aiServiceName} with payload {JsonSerializer.Serialize(aiServicePlan)}.");
                aiServicePlan = await _aIServicePlanService.UpdateAsync(aiServiceName, aiServicePlanName, aiServicePlan);
                return Ok(aiServicePlan);
            }
            else
            {
                _logger.LogInformation($"Create aiServicePlan {aiServicePlanName} in aiService {aiServiceName} with payload {JsonSerializer.Serialize(aiServicePlan)}.");
                await _aIServicePlanService.CreateAsync(aiServiceName, aiServicePlan);
                return CreatedAtRoute(nameof(GetAsync) + nameof(LunaAPI), new { aiServiceName = aiServiceName, aiServicePlanName = aiServicePlan.APIName }, aiServicePlan);
            }
        }

        /// <summary>
        /// Deletes a aiServicePlan within an aiService.
        /// </summary>
        /// <param name="aiServiceName">The name of the aiService.</param>
        /// <param name="aiServicePlanName">The name of the aiServicePlan to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("aiservices/{aiServiceName}/plans/{aiServicePlanName}")]
        [HttpDelete("applications/{aiServiceName}/apis/{aiServicePlanName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string aiServiceName, string aiServicePlanName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete aiServicePlan {aiServicePlanName} from aiService {aiServiceName}.");

            // check if there exist apiversions
            var apiVersions = await _apiVersionService.GetAllAsync(aiServiceName, aiServicePlanName);
            if (apiVersions.Count != 0)
            {
                throw new LunaConflictUserException($"Unable to delete {aiServicePlanName} with subscription");
            }

            await _aIServicePlanService.DeleteAsync(aiServiceName, aiServicePlanName);
            return NoContent();
        }
    }
}