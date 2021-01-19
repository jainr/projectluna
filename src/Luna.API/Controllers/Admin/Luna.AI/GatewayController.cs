using System;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Entities;
using Luna.Services.Data;
using Luna.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Luna.API.Controllers.Admin
{
    /// <summary>
    /// API controller for workspace resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class GatewayController : ControllerBase
    {
        private readonly IGatewayService _gatewayService;

        private readonly ILogger<AMLWorkspaceController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="gatewayService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public GatewayController(IGatewayService gatewayService, ILogger<AMLWorkspaceController> logger)
        {
            _gatewayService = gatewayService ?? throw new ArgumentNullException(nameof(gatewayService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all ai gateways.
        /// </summary>
        /// <returns>HTTP 200 OK with aiagent JSON objects in response body.</returns>
        [HttpGet("gateways")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation("Get all AI gateways.");
            return Ok(await _gatewayService.GetAllAsync());
        }

        /// <summary>
        /// Get an AI agent.
        /// </summary>
        /// <param name="name">The id of the agent to get.</param>
        /// <returns>HTTP 200 OK with ai agent JSON object in response body.</returns>
        [HttpGet("gateways/{name}", Name = nameof(GetAsync) + nameof(Gateway))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string name)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get AI agent name");
            return Ok(await _gatewayService.GetAsync(name));
        }

        /// <summary>
        /// Creates or updates an AI agent.
        /// </summary>
        /// <param name="name">The id of the AI agent to update.</param>
        /// <param name="gateway">The updated AI agent object.</param>
        /// <returns>HTTP 201 created or 200 for update.</returns>
        [HttpPut("gateways/{name}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string name, [FromBody] Gateway gateway)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, false);
            if (gateway == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(gateway)), UserErrorCode.PayloadNotProvided);
            }

            if (!name.Equals(gateway.Name))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(Gateway).Name),
                    UserErrorCode.NameMismatch);
            }

            if (string.IsNullOrEmpty(gateway.EndpointUrl))
            {
                throw new LunaBadRequestUserException("Endpoint url is required.", UserErrorCode.InvalidParameter);
            }

            if (string.IsNullOrEmpty(gateway.DisplayName))
            {
                throw new LunaBadRequestUserException("Display name is required.", UserErrorCode.InvalidParameter);
            }

            if (gateway.GatewayId == Guid.Empty)
            {
                throw new LunaBadRequestUserException("Gateway id (GUID) is required.", UserErrorCode.InvalidParameter);
            }

            gateway.CreatedBy = AADAuthHelper.GetUserAccount(this.HttpContext);

            if (await _gatewayService.ExistsAsync(name))
            {
                _logger.LogInformation($"Update AI agent {name}");
                await _gatewayService.UpdateAsync(name, gateway);
                return Ok(gateway);
            }
            else
            {
                _logger.LogInformation($"Create AI Agent {name}");
                await _gatewayService.CreateAsync(gateway);
                return CreatedAtRoute(nameof(GetAsync) + nameof(Gateway), new { name = name }, gateway);
            }
        }

        /// <summary>
        /// Deletes an AI agent.
        /// </summary>
        /// <param name="name">The id of the AI agent to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("gateways/{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string name)
        {
            _logger.LogInformation($"Delete AI agent {name}.");
            await _gatewayService.DeleteAsync(name);
            return NoContent();
        }
    }
}