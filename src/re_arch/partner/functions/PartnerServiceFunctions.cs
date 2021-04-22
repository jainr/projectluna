using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Luna.Partner.Clients.PartnerServiceClients;
using Luna.Partner.Data.Entities;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.HttpUtils;
using Luna.Common.Utils.LoggingUtils;
using Luna.Partner.PublicClient.DataContract;
using Luna.Common.Utils.Azure.AzureKeyvaultUtils;
using System.Linq;
using System.Net;
using System.Collections.Generic;

namespace Luna.RBAC
{
    /// <summary>
    /// The service maintains all RBAC rules
    /// </summary>
    public class PartnerServiceFunctions
    {
        private readonly IPartnerServiceClientFactory _serviceClientFactory;
        private readonly ISqlDbContext _dbContext;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;
        private readonly ILogger<PartnerServiceFunctions> _logger;

        public PartnerServiceFunctions(IPartnerServiceClientFactory serviceClientFactory, 
            IAzureKeyVaultUtils keyVaultUtils,
            ISqlDbContext dbContext, 
            ILogger<PartnerServiceFunctions> logger)
        {
            this._serviceClientFactory = serviceClientFactory;
            this._dbContext = dbContext;
            this._keyVaultUtils = keyVaultUtils;
            this._logger = logger;
        }


        /// <summary>
        /// Get a partner service
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="name">Name of the service</param>
        /// <returns></returns>
        [FunctionName("GetPartnerService")]
        public async Task<IActionResult> GetPartnerService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "partnerServices/{name}")] HttpRequest req, string name)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            try
            {
                var partnerServiceInternal = _dbContext.PartnerServices.
                    Where(p => p.UniqueName == name).SingleOrDefault<PartnerServiceInternal>();

                if (partnerServiceInternal == null)
                {
                    throw new LunaNotFoundUserException($"Partner service {name} doesn't exist.");
                }

                var configuration = await _keyVaultUtils.GetSecretAsync(partnerServiceInternal.ConfigurationSecretName);

                var partnerService = partnerServiceInternal.ToPublicCopy(configuration);

                return new OkObjectResult(partnerService);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        /// <summary>
        /// Get a partner service
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="name">Name of the service</param>
        /// <returns></returns>
        [FunctionName("ListPartnerServicesByType")]
        public async Task<IActionResult> ListPartnerServicesByType(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "partnerServices")] HttpRequest req)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            try
            {
                string type;
                if (req.Query.ContainsKey("type"))
                {
                    type = req.Query["type"];
                }
                else
                {
                    throw new LunaBadRequestUserException("The 'type' query parameter is required.", 
                        UserErrorCode.MissingQueryParameter);
                }

                var partnerServicesInternal = _dbContext.PartnerServices.Where(p => p.Type == type).ToList<PartnerServiceInternal>();
                List<PartnerService> results = new List<PartnerService>();
                foreach(var service in partnerServicesInternal)
                {
                    results.Add(service.ToPublicCopy(null));
                }

                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }


        /// <summary>
        /// Add a partner service
        /// </summary>
        /// <param name="req">The http request</param>
        /// <returns></returns>
        [FunctionName("AddPartnerService")]
        public async Task<IActionResult> AddPartnerService(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "partnerServices/{name}")] HttpRequest req,
            string name)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            try
            {
                PartnerService service = await HttpUtils.DeserializeRequestBody<PartnerService>(req);

                if (!name.Equals(service.UniqueName))
                {
                    throw new LunaBadRequestUserException("The name in URL does not match the name in request body.",
                        UserErrorCode.NameMismatch);
                }

                if (!_serviceClientFactory.GetPartnerServiceClient(service).TestConnection())
                {
                    throw new LunaBadRequestUserException("Can not connect to the partner service.", 
                        UserErrorCode.Disconnected);
                }

                var secretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.PARTNER_SERVICE_CONFIG);

                var configString = JsonConvert.SerializeObject(service.Configuration, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                await _keyVaultUtils.SetSecretAsync(secretName, configString);

                var serviceInternal = PartnerServiceInternal.CreateFrom(service);
                serviceInternal.ConfigurationSecretName = secretName;
                _dbContext.PartnerServices.Add(serviceInternal);
                await _dbContext._SaveChangesAsync();

                return new OkObjectResult(service);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        /// <summary>
        /// Remove a partner service
        /// </summary>
        /// <param name="req">The http request</param>
        /// <returns></returns>
        [FunctionName("RemovePartnerService")]
        public async Task<IActionResult> RemovePartnerService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "partnerServices/{name}")] HttpRequest req,
            string name)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            try
            {
                var partnerServiceInternal = _dbContext.PartnerServices.
                    Where(p => p.UniqueName == name).SingleOrDefault<PartnerServiceInternal>();

                if (partnerServiceInternal == null)
                {
                    throw new LunaNotFoundUserException($"Partner service {name} doesn't exist.");
                }
                _dbContext.PartnerServices.Remove(partnerServiceInternal);
                await _dbContext._SaveChangesAsync();

                return new NoContentResult();
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

    }
}
