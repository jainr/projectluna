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
    /// API controller for gitRepo resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class GitRepoController : ControllerBase
    {
        private readonly IGitRepoService _gitRepoService;

        private readonly ILogger<GitRepoController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="gitRepoService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public GitRepoController(IGitRepoService gitRepoService, ILogger<GitRepoController> logger)
        {
            _gitRepoService = gitRepoService ?? throw new ArgumentNullException(nameof(gitRepoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all gitRepos.
        /// </summary>
        /// <returns>HTTP 200 OK with gitRepo JSON objects in response body.</returns>
        [HttpGet("gitrepos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation("Get all gitRepos.");
            return Ok(await _gitRepoService.GetAllAsync());
        }

        /// <summary>
        /// Get an gitRepo.
        /// </summary>
        /// <param name="repoName">The name of the gitRepo to get.</param>
        /// <returns>HTTP 200 OK with gitRepo JSON object in response body.</returns>
        [HttpGet("gitrepos/{repoName}", Name = nameof(GetAsync) + nameof(GitRepo))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string repoName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get gitRepo {repoName}");
            return Ok(await _gitRepoService.GetAsync(repoName));
        }

        /// <summary>
        /// Creates or updates an gitRepo.
        /// </summary>
        /// <param name="repoName">The name of the gitRepo to update.</param>
        /// <param name="gitRepo">The updated gitRepo object.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpPut("gitrepos/{repoName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string repoName, [FromBody] GitRepo gitRepo)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (gitRepo == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(gitRepo)), UserErrorCode.PayloadNotProvided);
            }

            if (!repoName.Equals(gitRepo.RepoName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(GitRepo).Name),
                    UserErrorCode.NameMismatch);
            }

            if (await _gitRepoService.ExistsAsync(repoName))
            {
                _logger.LogInformation($"Update gitRepo {repoName} with payload {JsonConvert.SerializeObject(gitRepo)}");
                await _gitRepoService.UpdateAsync(repoName, gitRepo);
                return Ok(gitRepo);
            }
            else
            {
                _logger.LogInformation($"Create gitRepo {repoName} with payload {JsonConvert.SerializeObject(gitRepo)}");
                await _gitRepoService.CreateAsync(gitRepo);
                return CreatedAtRoute(nameof(GetAsync) + nameof(GitRepo), new { repoName = gitRepo.RepoName }, gitRepo);
            }
        }

        /// <summary>
        /// Deletes an gitRepo.
        /// </summary>
        /// <param name="repoName">The name of the gitRepo to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("gitrepos/{repoName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string repoName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete gitRepo {repoName}.");
            await _gitRepoService.DeleteAsync(repoName);
            return NoContent();
        }
    }
}