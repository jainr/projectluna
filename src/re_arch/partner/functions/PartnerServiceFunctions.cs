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
using Luna.Publish.PublicClient.Enums;

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
        /// Get ML host service types
        /// </summary>
        /// <group>Metadata</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/partnerServices/hostservicetypes</url>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="ServiceType"/>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetMLHostServiceTypes")]
        public async Task<IActionResult> GetMLHostServiceTypes(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "partnerServices/hostservicetypes")] HttpRequest req)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMLHostServiceTypes));

                try
                {
                    return new OkObjectResult(PartnerServiceTypeMetadata.MLHostServiceTypes);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetMLHostServiceTypes));
                }
            }
        }

        /// <summary>
        /// Get ML compute service types
        /// </summary>
        /// <group>Metadata</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/partnerServices/computeservicetypes</url>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="ServiceType"/>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetMLComputeServiceTypes")]
        public async Task<IActionResult> GetMLComputeServiceTypes(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "partnerServices/computeservicetypes")] HttpRequest req)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMLComputeServiceTypes));

                try
                {
                    return new OkObjectResult(PartnerServiceTypeMetadata.MLComputeServiceTypes);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetMLComputeServiceTypes));
                }
            }
        }

        /// <summary>
        /// Get ML component types
        /// </summary>
        /// <group>Metadata</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/partnerServices/{serviceType}/mlcomponenttypes</url>
        /// <param name="serviceType" required="true" cref="string" in="path">The ML host service type</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="ComponentType"/>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetMLComponentTypes")]
        public async Task<IActionResult> GetMLComponentTypes(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "partnerServices/{serviceType}/mlcomponenttypes")] HttpRequest req, string serviceType)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMLComponentTypes));

                try
                {
                    return new OkObjectResult(PartnerServiceComponentTypeMetadata.GetComponentTypes(serviceType));
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetMLComponentTypes));
                }
            }
        }

        /// <summary>
        /// Get ML components by type
        /// </summary>
        /// <group>Metadata</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/partnerServices/{serviceName}/mlcomponents/{componentType}</url>
        /// <param name="serviceName" required="true" cref="string" in="path">The partner service name</param>
        /// <param name="componentType" required="true" cref="string" in="path">The ML component type</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="BaseMLComponent"/>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetMLComponents")]
        public async Task<IActionResult> GetMLComponents(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "partnerServices/{serviceName}/mlcomponents/{componentType}")] 
            HttpRequest req, 
            string serviceName,
            string componentType)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetMLComponents));

                try
                {
                    var partnerServiceInternal = _dbContext.PartnerServices.
                        Where(p => p.UniqueName == serviceName).SingleOrDefault<PartnerServiceInternal>();

                    if (partnerServiceInternal == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.PARTNER_SERVICE_DOES_NOT_EXIST, serviceName));
                    }

                    var configuration = await _keyVaultUtils.GetSecretAsync(partnerServiceInternal.ConfigurationSecretName);

                    var config = JsonConvert.DeserializeObject<BasePartnerServiceConfiguration>(configuration,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        });

                    if (componentType.Equals(LunaAPIType.Realtime.ToString(), 
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        var client = _serviceClientFactory.GetRealtimeEndpointPartnerServiceClient(serviceName, config);

                        if (client != null)
                        {
                            return new OkObjectResult(await client.ListRealtimeEndpointsAsync());
                        }
                    }
                    else if (componentType.Equals(LunaAPIType.Pipeline.ToString(),
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        var client = _serviceClientFactory.GetPipelineEndpointPartnerServiceClient(serviceName, config);

                        if (client != null)
                        {
                            return new OkObjectResult(await client.ListPipelineEndpointsAsync());
                        }
                    }

                    throw new LunaNotFoundUserException(
                        string.Format(ErrorMessages.INVALID_ML_COMPONENT_TYPE, serviceName, componentType));
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetMLComponents));
                }
            }
        }


        /// <summary>
        /// Get a partner service
        /// </summary>
        /// <group>Partner service</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/partnerServices/{name}</url>
        /// <param name="req">The http request</param>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <response code="200"><see cref="BasePartnerServiceConfiguration"/>Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetPartnerService")]
        public async Task<IActionResult> GetPartnerService(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "partnerServices/{name}")] HttpRequest req, string name)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetPartnerService));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetPartnerService));
                }
            }
        }

        /// <summary>
        /// List partner services by type
        /// </summary>
        /// <group>Partner service</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/partnerServices</url>
        /// <param name="req">The http request</param>
        /// <param name="type" cref="string" in="query">Partner service type</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="BasePartnerServiceConfiguration"/>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListPartnerServicesByType")]
        public async Task<IActionResult> ListPartnerServicesByType(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "partnerServices")] HttpRequest req)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListPartnerServicesByType));

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
                    foreach (var service in partnerServicesInternal)
                    {
                        results.Add(service.ToPublicPartnerService());
                    }

                    return new OkObjectResult(results);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListPartnerServicesByType));
                }
            }
        }

        /// <summary>
        /// Update a partner service
        /// </summary>
        /// <group>Partner service</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/partnerServices/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req" in="body">
        ///     <see cref="BasePartnerServiceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BasePartnerServiceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure ML workspace as partner service
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200"><see cref="BasePartnerServiceConfiguration"/>Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdatePartnerService")]
        public async Task<IActionResult> UpdatePartnerService(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "partnerServices/{name}")] HttpRequest req,
            string name)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdatePartnerService));

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

                    if (!await _serviceClientFactory.GetPartnerServiceClient(name, config).TestConnectionAsync())
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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdatePartnerService));
                }
            }
        }

        /// <summary>
        /// Add a partner service
        /// </summary>
        /// <group>Partner service</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/partnerServices/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <param name="req" in="body">
        ///     <see cref="BasePartnerServiceConfiguration"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BasePartnerServiceConfiguration.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Azure ML workspace as partner service
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200"><see cref="BasePartnerServiceConfiguration"/>Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("AddPartnerService")]
        public async Task<IActionResult> AddPartnerService(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "partnerServices/{name}")] HttpRequest req,
            string name)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.AddPartnerService));

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

                    if (!await _serviceClientFactory.GetPartnerServiceClient(name, config).TestConnectionAsync())
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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.AddPartnerService));
                }
            }
        }

        /// <summary>
        /// Remove a partner service
        /// </summary>
        /// <group>Partner service</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/partnerServices/{name}</url>
        /// <param name="req">The http request</param>
        /// <param name="name" required="true" cref="string" in="path">Name of the partner service</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("RemovePartnerService")]
        public async Task<IActionResult> RemovePartnerService(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "partnerServices/{name}")] HttpRequest req,
            string name)
        {
            LunaRequestHeaders lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RemovePartnerService));

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
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RemovePartnerService));
                }
            }
        }

    }
}
