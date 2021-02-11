using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.DataContracts.Luna.AI;
using Luna.Data.Entities;
using Luna.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for the apiVersion resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class APIVersionController : ControllerBase
    {
        private readonly IAPIVersionService _apiVersionService;
        private readonly ILogger<APIVersionController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="apiVersionService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public APIVersionController(IAPIVersionService apiVersionService, ILogger<APIVersionController> logger)
        {
            _apiVersionService = apiVersionService ?? throw new ArgumentNullException(nameof(apiVersionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all apiVersions within a ai service plan within an ai service.
        /// </summary>
        /// <param name="aiServiceName">The name of the ai service.</param>
        /// <param name="aiServicePlanName">The name of the ai service plan.</param>
        /// <returns>HTTP 200 OK with apiVersions JSON objects in response body.</returns>
        [HttpGet("applications/{aiServiceName}/apis/{aiServicePlanName}/apiVersions")]
        [HttpGet("aiservices/{aiServiceName}/plans/{aiServicePlanName}/apiVersions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync(string aiServiceName, string aiServicePlanName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get all apiVersions in ai service plan {aiServicePlanName} in ai service {aiServiceName}.");
            return Ok(await _apiVersionService.GetAllAsync(aiServiceName, aiServicePlanName));
        }

        /// <summary>
        /// Get an apiVersion
        /// </summary>
        /// <param name="aiServiceName">The name of the ai service.</param>
        /// <param name="aiServicePlanName">The name of the ai service plan.</param>
        /// <param name="versionName">The name of apiversion</param>
        /// <returns>HTTP 200 OK with one apiVersion JSON objects in response body.</returns>
        [HttpGet("aiservices/{aiServiceName}/plans/{aiServicePlanName}/apiVersions/{versionName}", Name = nameof(GetAsync) + nameof(APIVersion))]
        [HttpGet("applications/{aiServiceName}/apis/{aiServicePlanName}/apiVersions/{versionName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string aiServiceName, string aiServicePlanName, string versionName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get apiVersion {versionName.ToString()} in ai service plan {aiServicePlanName} in ai service {aiServiceName}.");
            return Ok(await _apiVersionService.GetAsync(aiServiceName, aiServicePlanName, versionName));
        }

        /// <summary>
        /// Creates pr update a apiVersion within a ai service plan within an ai service.
        /// </summary>
        /// <param name="aiServiceName">The name of the ai service.</param>
        /// <param name="aiServicePlanName">The name of the ai service plan.</param>
        /// <param name="versionName">The name of apiversion</param>
        /// <param name="apiVersion">The apiVersion object to create.</param>
        /// <returns>HTTP 201 CREATED with URI to created resource in response header.</returns>
        /// <returns>HTTP 200 OK with updated apiVersion JSON objects in response body.</returns>
        [HttpPut("aiservices/{aiServiceName}/plans/{aiServicePlanName}/apiVersions/{versionName}")]
        [HttpPut("applications/{aiServiceName}/apis/{aiServicePlanName}/apiVersions/{versionName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string aiServiceName, string aiServicePlanName, string versionName, [FromBody] APIVersion apiVersion)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (apiVersion == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(apiVersion)), UserErrorCode.PayloadNotProvided);
            }

            if (!versionName.Equals(apiVersion.VersionName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.NameMismatch);
            }

            if(await _apiVersionService.ExistsAsync(aiServiceName, aiServicePlanName, versionName))
            {
                _logger.LogInformation($"Update apiVersion {versionName} in ai service plan {aiServicePlanName} in ai service {aiServiceName} with payload {JsonSerializer.Serialize(apiVersion)}.");
                apiVersion = await _apiVersionService.UpdateAsync(aiServiceName, aiServicePlanName, versionName, apiVersion);
                return Ok(apiVersion);
            }
            else
            {
                _logger.LogInformation($"Create apiVersion {versionName} in ai service plan {aiServicePlanName} in ai service {aiServiceName} with payload {JsonSerializer.Serialize(apiVersion)}.");
                await _apiVersionService.CreateAsync(aiServiceName, aiServicePlanName, apiVersion);
                return CreatedAtRoute(nameof(GetAsync) + nameof(APIVersion), new { aiServiceName = aiServiceName, aiServicePlanName = aiServicePlanName, versionName = versionName }, apiVersion);
            }
        }


        /// <summary>
        /// Delete an apiVersion
        /// </summary>
        /// <param name="aiServiceName">The name of the ai service.</param>
        /// <param name="aiServicePlanName">The name of the ai service plan.</param>
        /// <param name="versionName">The name of apiversion</param>
        /// <returns>HTTP 204 NO CONTENT</returns>
        [HttpDelete("aiservices/{aiServiceName}/plans/{aiServicePlanName}/apiVersions/{versionName}")]
        [HttpDelete("applications/{aiServiceName}/apis/{aiServicePlanName}/apiVersions/{versionName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string aiServiceName, string aiServicePlanName, string versionName)
        {

            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete apiVersion {versionName.ToString()} in ai service plan {aiServicePlanName} in ai service {aiServiceName}.");
            await _apiVersionService.DeleteAsync(aiServiceName, aiServicePlanName, versionName);
            return NoContent();
        }
    }
}