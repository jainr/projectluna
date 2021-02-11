using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.DataContracts.Luna.AI;
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
    /// API controller for product resource.
    /// </summary>
    [ApiController]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api")]
    public class AIServiceController : ControllerBase
    {
        private readonly ILunaApplicationService _productService;

        private readonly ILogger<AIServiceController> _logger;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="productService">The service to inject.</param>
        /// <param name="logger">The logger.</param>
        public AIServiceController(ILunaApplicationService productService, ILogger<AIServiceController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>HTTP 200 OK with product JSON objects in response body.</returns>
        [HttpGet("aiservices")]
        [HttpGet("products")]
        [HttpGet("applications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllAsync()
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation("Get all AI Services.");
            return Ok(await _productService.GetAllAsync());
            
        }

        /// <summary>
        /// Get a product.
        /// </summary>
        /// <param name="aiServiceName">The name of the product to get.</param>
        /// <returns>HTTP 200 OK with product JSON object in response body.</returns>
        [HttpGet("products/{aiServiceName}")]
        [HttpGet("applications/{aiServiceName}")]
        [HttpGet("aiservices/{aiServiceName}", Name = nameof(GetAsync) + nameof(LunaApplication))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAsync(string aiServiceName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Get AI Service {aiServiceName}");
            return Ok(await _productService.GetAsync(aiServiceName));
        }

        /// <summary>
        /// Creates or updates an product.
        /// </summary>
        /// <param name="aiServiceName">The name of the product to update.</param>
        /// <param name="aiService">The updated product object.</param>
        /// <returns>HTTP 201 CREATED with URI to created resource in response header.</returns>
        /// <returns>HTTP 200 OK with updated product JSON objects in response body.</returns>
        [HttpPut("products/{aiServiceName}")]
        [HttpPut("aiservices/{aiServiceName}")]
        [HttpPut("applications/{aiServiceName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateOrUpdateAsync(string aiServiceName, [FromBody] LunaApplication aiService)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            if (aiService == null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(nameof(aiService)), UserErrorCode.PayloadNotProvided);
            }

            if (!aiServiceName.Equals(aiService.ApplicationName))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(LunaApplication).Name),
                    UserErrorCode.NameMismatch);
            }

            if (!ControllerHelper.ValidateStringFormat(aiServiceName, ValidStringFormat.LOWER_CASE_NUMBER_UNDERSCORE_AND_HYPHEN_50))
            {
                throw new LunaBadRequestUserException($"The AI Service name is invalid. The naming rule: {ControllerHelper.GetStringFormatDescription(ValidStringFormat.LOWER_CASE_NUMBER_AND_HYPHEN_50)}", 
                    UserErrorCode.InvalidParameter);
            }

            if (await _productService.ExistsAsync(aiServiceName))
            {
                _logger.LogInformation($"Update AI Service {aiServiceName} with payload {JsonConvert.SerializeObject(aiService)}");
                aiService = await _productService.UpdateAsync(aiServiceName, aiService);
                return Ok(aiService);
            }
            else
            {
                _logger.LogInformation($"Create AI Service {aiServiceName} with payload {JsonConvert.SerializeObject(aiService)}");
                await _productService.CreateAsync(aiService);
                return CreatedAtRoute(nameof(GetAsync) + nameof(LunaApplication), new { aiServiceName = aiService.ApplicationName }, aiService);
            }
        }

        /// <summary>
        /// Deletes an product.
        /// </summary>
        /// <param name="aiServiceName">The name of the product to delete.</param>
        /// <returns>HTTP 204 NO CONTENT.</returns>
        [HttpDelete("aiservices/{aiServiceName}")]
        [HttpDelete("products/{aiServiceName}")]
        [HttpDelete("applications/{aiServiceName}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAsync(string aiServiceName)
        {
            AADAuthHelper.VerifyUserAccess(this.HttpContext, _logger, true);
            _logger.LogInformation($"Delete AI Service {aiServiceName}.");
            await _productService.DeleteAsync(aiServiceName);
            return NoContent();
        }
    }
}