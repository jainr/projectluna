using System;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
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
    public class AzureDatabricksWorkspaceController : ControllerBase
    {
        private readonly IAzureDatabricksWorkspaceService _workspaceService;

        private readonly ILogger<AzureDatabricksWorkspaceController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="workspaceService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public AzureDatabricksWorkspaceController(IAzureDatabricksWorkspaceService workspaceService, ILogger<AzureDatabricksWorkspaceController> logger)
        {
            _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all workspaces.
        /// </summary>
        /// <returns>HTTP 200 OK with workspace JSON objects in response body.</returns>
        [HttpGet("azuredatabricksworkspaces")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation("Get all workspaces.");
            return Ok(await _workspaceService.GetAllAsync());
        }

        /// <summary>
        /// Get an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to get.</param>
        /// <returns>HTTP 200 OK with workspace JSON object in response body.</returns>
        [HttpGet("azuredatabricksworkspaces/{workspaceName}", Name = nameof(GetAsync) + nameof(AzureDatabricksWorkspace))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string workspaceName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get workspace {workspaceName}");
            return Ok(await _workspaceService.GetAsync(workspaceName));
        }

        /// <summary>
        /// Creates or updates an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to update.</param>
        /// <param name="workspace">The updated workspace object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("azuredatabricksworkspaces/{workspaceName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string workspaceName, [FromBody] AzureDatabricksWorkspace workspace)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (workspace == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(workspace)), UserErrorCode.PayloadNotProvided);
            }

            if (!workspaceName.Equals(workspace.WorkspaceName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AzureDatabricksWorkspace).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _workspaceService.ExistsAsync(workspaceName))
            {
                _logger.LogInformation($"Update workspace {workspaceName} with payload {JsonConvert.SerializeObject(workspace)}");
                await _workspaceService.UpdateAsync(workspaceName, workspace);
                return Ok(workspace);
            }
            else
            {
                _logger.LogInformation($"Create workspace {workspaceName} with payload {JsonConvert.SerializeObject(workspace)}");
                await _workspaceService.CreateAsync(workspace);
                return CreatedAtRoute(nameof(GetAsync) + nameof(AzureDatabricksWorkspace), new { workspaceName = workspace.WorkspaceName }, workspace);
            }
        }

        /// <summary>
        /// Deletes an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("azuredatabricksworkspaces/{workspaceName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string workspaceName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete workspace {workspaceName}.");
            await _workspaceService.DeleteAsync(workspaceName);
            return NoContent();
        }
    }
}