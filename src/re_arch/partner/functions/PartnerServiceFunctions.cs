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
using Luna.Common.Utils.RestClients;
using Luna.Common.Utils.Azure.AzureKeyvaultUtils;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Luna.Common.LoggingUtils;
using Microsoft.EntityFrameworkCore;

namespace Luna.Partner.Functions
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
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "partnerServices/{name}")] HttpRequest req, string name)
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

                var config = JsonConvert.DeserializeObject<BasePartnerServiceConfiguration>(configuration,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                return new OkObjectResult(config);
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
        /// <returns></returns>
        [FunctionName("ListPartnerServicesByType")]
        public async Task<IActionResult> ListPartnerServicesByType(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "partnerServices")] HttpRequest req)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            try
            {
                string type;
                if (req.Query.ContainsKey(PartnerQueryParameterConstats.PARTNER_SERVICE_TYPE_QUERY_PARAM_NAME))
                {
                    type = req.Query[PartnerQueryParameterConstats.PARTNER_SERVICE_TYPE_QUERY_PARAM_NAME];
                }
                else
                {
                    throw new LunaBadRequestUserException(
                        string.Format(ErrorMessages.MISSING_QUERY_PARAMETER, PartnerQueryParameterConstats.PARTNER_SERVICE_TYPE_QUERY_PARAM_NAME), 
                        UserErrorCode.MissingQueryParameter);
                }

                var partnerServicesInternal = _dbContext.PartnerServices.Where(p => p.Type == type).ToList<PartnerServiceInternal>();
                List<PartnerService> results = new List<PartnerService>();
                foreach(var service in partnerServicesInternal)
                {
                    results.Add(service.ToPublicPartnerService());
                }

                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }


        /// <summary>
        /// Update a partner service
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="name">The name of the partner service</param>
        /// <returns></returns>
        [FunctionName("UpdatePartnerService")]
        public async Task<IActionResult> UpdatePartnerService(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "partnerServices/{name}")] HttpRequest req,
            string name)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            try
            {
                var requestBody = await HttpUtils.GetRequestBodyAsync(req);

                var config = JsonConvert.DeserializeObject<BasePartnerServiceConfiguration>(requestBody,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                var currentService = await _dbContext.PartnerServices.SingleOrDefaultAsync(x => x.UniqueName == name);

                if (currentService == null)
                {
                    throw new LunaNotFoundUserException(
                        string.Format(ErrorMessages.PARTNER_SERVICE_DOES_NOT_EXIST, name));
                }

                if (!currentService.Type.Equals(config.Type))
                {
                    throw new LunaConflictUserException(
                        string.Format(ErrorMessages.CAN_NOT_UPDATE_PARTNER_SERVICE_TYPE, currentService.Type));
                }

                if (!_serviceClientFactory.GetPartnerServiceClient(name, config).TestConnection())
                {
                    throw new LunaBadRequestUserException(
                        string.Format(ErrorMessages.CAN_NOT_CONNECT_TO_PARTNER_SERVICE, name),
                        UserErrorCode.Disconnected);
                }

                await _keyVaultUtils.SetSecretAsync(currentService.ConfigurationSecretName, requestBody);

                currentService.UpdateFromConfig(config);
                _dbContext.PartnerServices.Update(currentService);
                await _dbContext._SaveChangesAsync();

                return new OkObjectResult(config);
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
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "partnerServices/{name}")] HttpRequest req,
            string name)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            try
            {
                var requestBody = await HttpUtils.GetRequestBodyAsync(req);

                var config = JsonConvert.DeserializeObject<BasePartnerServiceConfiguration>(requestBody, 
                    new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

                if (await _dbContext.PartnerServices.AnyAsync(x => x.UniqueName == name))
                {
                    throw new LunaConflictUserException(string.Format(ErrorMessages.PARTNER_SERVICE_ALREADY_EXIST, name));
                }

                if (!_serviceClientFactory.GetPartnerServiceClient(name, config).TestConnection())
                {
                    throw new LunaBadRequestUserException("Can not connect to the partner service.", 
                        UserErrorCode.Disconnected);
                }

                var secretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.PARTNER_SERVICE_CONFIG);

                await _keyVaultUtils.SetSecretAsync(secretName, requestBody);

                var serviceInternal = PartnerServiceInternal.CreateFromConfig(name, config);
                serviceInternal.ConfigurationSecretName = secretName;
                _dbContext.PartnerServices.Add(serviceInternal);
                await _dbContext._SaveChangesAsync();

                return new OkObjectResult(config);
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
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "partnerServices/{name}")] HttpRequest req,
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
