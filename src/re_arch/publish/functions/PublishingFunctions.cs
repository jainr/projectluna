using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Luna.Publish.Clients.EventGenerator;
using Luna.Publish.Clients.HttpRequestParser;
using Luna.Publish.Data.Entities;
using Luna.Publish.Data.Enums;
using Luna.Publish.Clients.EventProcessor;
using System.Collections.Generic;
using Luna.Publish.Data.DataContracts.Events;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Luna.Publish.PublicClient.Enums;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Common.Utils.LoggingUtils;
using Luna.Common.Utils.HttpUtils;
using Luna.Common.LoggingUtils;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.Azure.AzureKeyvaultUtils;
using Luna.PubSub.PublicClient;
using Luna.PubSub.PublicClient.Clients;
using Luna.Publish.Public.Client.DataContract;

namespace Luna.Publish.Functions
{
    /// <summary>
    /// The service maintains all Luna application, APIs and API versions
    /// </summary>
    public class PublishingFunctions
    {

        private readonly IPublishingEventContentGenerator _eventGenerator;
        private readonly IPublishingEventProcessor _eventProcessor;
        private readonly IHttpRequestParser _requestParser;
        private readonly ISqlDbContext _dbContext;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;
        private readonly IPubSubServiceClient _pubSubClient;
        private readonly ILogger<PublishingFunctions> _logger;

        public PublishingFunctions(IPublishingEventContentGenerator eventGenerator, 
            IPublishingEventProcessor eventProcessor,
            IHttpRequestParser parser,
            ISqlDbContext dbContext,
            IAzureKeyVaultUtils keyVaultUtils,
            IPubSubServiceClient pubSubClient,
            ILogger<PublishingFunctions> logger)
        {
            this._eventGenerator = eventGenerator ?? throw new ArgumentNullException(nameof(eventGenerator));
            this._eventProcessor = eventProcessor ?? throw new ArgumentNullException(nameof(eventProcessor));
            this._requestParser = parser ?? throw new ArgumentNullException(nameof(parser));
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
            this._pubSubClient = pubSubClient ?? throw new ArgumentNullException(nameof(pubSubClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Luna application operations

        /// <summary>
        /// List all applications
        /// </summary>
        /// <group>Application</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applicationss</url>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="LunaApplication"/>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListLunaApplications")]
        public async Task<IActionResult> ListLunaApplications(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "applications")] HttpRequest req)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);

            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListLunaApplications));
                try
                {
                    List<LunaApplication> apps = new List<LunaApplication>();

                    // Return all applications only when "role=admin" is specified in the query parameter
                    if (req.Query.ContainsKey(PublishQueryParameterConstants.ROLE_QUERY_PARAMETER_NAME) &&
                        req.Query[PublishQueryParameterConstants.ROLE_QUERY_PARAMETER_NAME].ToString().Equals(PublishQueryParameterConstants.ADMIN_ROLE_NAME))
                    {
                        apps = await _dbContext.LunaApplications.
                            Select(x => x.GetLunaApplication()).
                            ToListAsync();
                    }
                    else
                    {
                        apps = await _dbContext.LunaApplications.
                            Where(x => x.OwnerUserId == lunaHeaders.UserId).
                            Select(x => x.GetLunaApplication()).
                            ToListAsync();
                    }

                    return new OkObjectResult(apps);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListLunaApplications));
                }
            }
        }

        /// <summary>
        /// Get Luna application master keys
        /// </summary>
        /// <group>Application</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applicationss/{name}/masterkeys</url>
        /// <param name="name" required="true" cref="string" in="path">The name of the application</param>
        /// <response code="200">
        ///     <see cref="LunaApplicationMasterKeys"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationMasterKeys.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application master keys
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetLunaApplicationMasterKeys")]
        public async Task<IActionResult> GetLunaApplicationMasterKeys(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "applications/{name}/masterkeys")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetLunaApplicationMasterKeys));

                try
                {
                    var app = await _dbContext.LunaApplications.SingleOrDefaultAsync(x => x.ApplicationName == name);
                    if (app != null)
                    {
                        var keys = new LunaApplicationMasterKeys()
                        {
                            PrimaryMasterKey = await _keyVaultUtils.GetSecretAsync(app.PrimaryMasterKeySecretName),
                            SecondaryMasterKey = await _keyVaultUtils.GetSecretAsync(app.SecondaryMasterKeySecretName)
                        };
                        return new OkObjectResult(keys);
                    }
                    else
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, name));
                    }
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetLunaApplicationMasterKeys));
                }
            }
        }

        /// <summary>
        /// Get a Luna application
        /// </summary>
        /// <group>Application</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applicationss/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">The name of the application</param>
        /// <response code="200">
        ///     <see cref="LunaApplication"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplication.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("GetLunaApplication")]
        public async Task<IActionResult> GetLunaApplication(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.GetLunaApplication));

                try
                {
                    if (!await IsApplicationExist(name))
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, name));
                    }

                    var snapshot = _dbContext.ApplicationSnapshots.
                        Where(x => x.ApplicationName == name).
                        OrderByDescending(x => x.LastAppliedEventId).FirstOrDefault();

                    var events = await _dbContext.PublishingEvents.
                        Where(x => x.Id > snapshot.LastAppliedEventId).
                        OrderBy(x => x.Id).
                        Select(x => x.GetEventObject()).
                        ToListAsync();

                    var lunaApp = _eventProcessor.GetLunaApplication(name, events, snapshot);

                    if (lunaApp == null)
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, name));
                    }

                    return new OkObjectResult(lunaApp);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.GetLunaApplication));
                }
            }
        }

        /// <summary>
        /// Create a Luna application
        /// </summary>
        /// <group>Application</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/applications/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application properties
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application properties
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateLunaApplication")]
        public async Task<IActionResult> CreateLunaApplication(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateLunaApplication));

                try
                {
                    if (await IsApplicationExist(name))
                    {
                        throw new LunaConflictUserException(
                            string.Format(ErrorMessages.APPLICATION_ALREADY_EXIST, name));
                    }

                    var application = await _requestParser.ParseAndValidateLunaApplicationAsync(
                        await HttpUtils.GetRequestBodyAsync(req));
                    application.CreatedBy = lunaHeaders.UserId;
                    application.PrimaryMasterKeySecretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.APPLICATION_MASTER_KEY);
                    application.SecondaryMasterKeySecretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.APPLICATION_MASTER_KEY);

                    await _keyVaultUtils.SetSecretAsync(application.PrimaryMasterKeySecretName, Guid.NewGuid().ToString("N"));
                    await _keyVaultUtils.SetSecretAsync(application.SecondaryMasterKeySecretName, Guid.NewGuid().ToString("N"));

                    var ev = _eventGenerator.GenerateCreateLunaApplicationEventContent(name, application);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = name,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.CreateLunaApplication.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    var snapshot = await this.CreateSnapshot(name,
                        ApplicationStatus.Draft,
                        currentEvent: publishingEvent,
                        isNewApp: true);

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.LunaApplications.Add(
                            new LunaApplicationDB(
                                name,
                                lunaHeaders.UserId,
                                application.PrimaryMasterKeySecretName,
                                application.SecondaryMasterKeySecretName));
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();

                        snapshot.LastAppliedEventId = publishingEvent.Id;
                        _dbContext.ApplicationSnapshots.Add(snapshot);
                        await _dbContext._SaveChangesAsync();

                        transaction.Commit();
                    }

                    return new OkObjectResult(application);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateLunaApplication));
                }
            }
        }


        /// <summary>
        /// Update a Luna application
        /// </summary>
        /// <group>Application</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/applications/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="req" in="body">
        ///     <see cref="LunaApplicationProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application properties
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application properties
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateLunaApplication")]
        public async Task<IActionResult> UpdateLunaApplication(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateLunaApplication));

                try
                {
                    var appToUpdate = await _dbContext.LunaApplications.FindAsync(name);
                    if (appToUpdate == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, name));
                    }

                    var application = await _requestParser.ParseAndValidateLunaApplicationAsync(
                        await HttpUtils.GetRequestBodyAsync(req));
                    var ev = _eventGenerator.GenerateUpdateLunaApplicationEventContent(name, application);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = name,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.UpdateLunaApplication.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        appToUpdate.LastUpdatedTime = DateTime.UtcNow;
                        _dbContext.LunaApplications.Update(appToUpdate);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();


                        transaction.Commit();
                    }

                    return new OkObjectResult(application);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateLunaApplication));
                }
            }
        }

        /// <summary>
        /// Delete a Luna application
        /// </summary>
        /// <group>Application</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/applications/{name}</url>
        /// <param name="name" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="req">The http request</param>
        /// <response code="204">
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteLunaApplication")]
        public async Task<IActionResult> DeleteLunaApplication(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "applications/{name}")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteLunaApplication));

                try
                {
                    var appToDelete = await _dbContext.LunaApplications.FindAsync(name);
                    if (appToDelete == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, name));
                    }

                    if (await IsAnyAPIExist(name))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.CAN_NOT_DELETE_APPLICATION_WITH_APIS, name));
                    }

                    var ev = _eventGenerator.GenerateDeleteLunaApplicationEventContent(name);

                    var applicationEvent = new DeleteApplicationEventEntity(name, ev);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = name,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.DeleteLunaApplication.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    var snapshot = await this.CreateSnapshot(name,
                        ApplicationStatus.Deleted,
                        currentEvent: publishingEvent);

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.LunaApplications.Remove(appToDelete);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();

                        snapshot.LastAppliedEventId = publishingEvent.Id;
                        _dbContext.ApplicationSnapshots.Add(snapshot);
                        await _dbContext._SaveChangesAsync();

                        await _pubSubClient.PublishEventAsync(LunaEventStoreType.APPLICATION_EVENT_STORE, applicationEvent, lunaHeaders);

                        transaction.Commit();
                    }

                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteLunaApplication));
                }
            }
        }


        /// <summary>
        /// Regenerate Luna application master keys
        /// </summary>
        /// <group>Application</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/applications/{name}/regenerateMasterKeys</url>
        /// <param name="name" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="key-name" required="true" cref="string" in="query">
        ///     <example>
        ///         <value>
        ///             PrimaryKey
        ///         </value>
        ///         <summary>
        ///             An example of Luna application master key name
        ///         </summary>
        ///     </example>
        ///     The key name. Valid values are PrimaryKey or SecondaryKey
        /// </param>
        /// <response code="200">
        ///     <see cref="LunaApplicationMasterKeys"/>
        ///     <example>
        ///         <value>
        ///             <see cref="LunaApplicationMasterKeys.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna application master keys
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("RegenerateApplicationMasterKey")]
        public async Task<IActionResult> RegenerateApplicationMasterKey(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "applications/{name}/regenerateMasterKeys")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.RegenerateApplicationMasterKey));

                try
                {
                    if (!req.Query.ContainsKey(PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME))
                    {
                        throw new LunaBadRequestUserException(string.Format(ErrorMessages.MISSING_QUERY_PARAMETER, PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME),
                            UserErrorCode.MissingQueryParameter);
                    }
                    var keyName = req.Query[PublishQueryParameterConstants.KEY_NAME_QUERY_PARAMETER_NAME].ToString();

                    var app = await _dbContext.LunaApplications.SingleOrDefaultAsync(x => x.ApplicationName == name);
                    if (app != null)
                    {
                        if (!keyName.Equals(PublishQueryParameterConstants.PRIMARY_KEY_NAME) && !keyName.Equals(PublishQueryParameterConstants.SECONDARY_KEY_NAME))
                        {
                            throw new LunaNotFoundUserException(
                                string.Format(ErrorMessages.APPLICATION_KEY_DOES_NOT_EXIST, keyName));
                        }

                        // Create regenerate key event first
                        // In case the key regeneration failed, the event consumer will just get the same key (do no harm)
                        var eventEntity = new RegenerateApplicationMasterKeyEventEntity(name, JsonConvert.SerializeObject(new { KeyName = keyName }));
                        await _pubSubClient.PublishEventAsync(LunaEventStoreType.APPLICATION_EVENT_STORE, eventEntity, lunaHeaders);

                        if (keyName.Equals(PublishQueryParameterConstants.PRIMARY_KEY_NAME))
                        {
                            await _keyVaultUtils.SetSecretAsync(app.PrimaryMasterKeySecretName, Guid.NewGuid().ToString("N"));
                        }
                        else if (keyName.Equals(PublishQueryParameterConstants.SECONDARY_KEY_NAME))
                        {
                            await _keyVaultUtils.SetSecretAsync(app.SecondaryMasterKeySecretName, Guid.NewGuid().ToString("N"));
                        }
                        else
                        {
                            throw new LunaNotSupportedUserException(string.Format(ErrorMessages.KEY_NAME_NOT_SUPPORTED, keyName));
                        }

                        var keys = new LunaApplicationMasterKeys()
                        {
                            PrimaryMasterKey = await _keyVaultUtils.GetSecretAsync(app.PrimaryMasterKeySecretName),
                            SecondaryMasterKey = await _keyVaultUtils.GetSecretAsync(app.SecondaryMasterKeySecretName)
                        };

                        return new OkObjectResult(keys);
                    }
                    else
                    {
                        throw new LunaNotFoundUserException(
                            string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, name));
                    }
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.RegenerateApplicationMasterKey));
                }
            }
        }


        /// <summary>
        /// Publish a Luna application
        /// </summary>
        /// <group>Application</group>
        /// <verb>POST</verb>
        /// <url>http://localhost:7071/api/applications/{name}/publish</url>
        /// <param name="name" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="req">The http request</param>
        /// <response code="204">
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("PublishLunaApplication")]
        public async Task<IActionResult> PublishLunaApplication(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "applications/{name}/publish")] HttpRequest req,
            string name)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.PublishLunaApplication));

                try
                {
                    var appToPublish = await _dbContext.LunaApplications.FindAsync(name);

                    if (appToPublish == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, name));
                    }

                    var comments = "";
                    if (req.Query.ContainsKey("comments"))
                    {
                        comments = req.Query["comments"];
                    }


                    var ev = _eventGenerator.GeneratePublishLunaApplicationEventContent(name, comments);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = name,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.PublishLunaApplication.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    var snapshot = await this.CreateSnapshot(name,
                        ApplicationStatus.Published,
                        currentEvent: publishingEvent);

                    var applicationEvent = new PublishApplicationEventEntity(name, snapshot.SnapshotContent);

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        appToPublish.Status = ApplicationStatus.Published.ToString();
                        appToPublish.LastPublishedTime = DateTime.UtcNow;
                        _dbContext.LunaApplications.Update(appToPublish);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();

                        snapshot.LastAppliedEventId = publishingEvent.Id;
                        _dbContext.ApplicationSnapshots.Add(snapshot);
                        await _dbContext._SaveChangesAsync();

                        await _pubSubClient.PublishEventAsync(LunaEventStoreType.APPLICATION_EVENT_STORE, applicationEvent, lunaHeaders);

                        transaction.Commit();
                    }

                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.PublishLunaApplication));
                }
            }
        }

        #endregion

        #region Luna API operations

        /// <summary>
        /// List Luna APIs in the specified application
        /// </summary>
        /// <group>API</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applications/{name}/apis</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="LunaAPI"/>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListLunaAPIs")]
        public async Task<IActionResult> ListLunaAPIs(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "applications/{appName}/apis")] HttpRequest req,
            string appName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListLunaAPIs));

                try
                {
                    var apis = await _dbContext.LunaAPIs.
                        Select(x => x.GetLunaAPI()).
                        ToListAsync();

                    return new OkObjectResult(apis);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListLunaAPIs));
                }
            }
        }

        /// <summary>
        /// Create an API in the specified Luna application
        /// </summary>
        /// <group>API</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/apis/{apiName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of API</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseLunaAPIProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseLunaAPIProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna API
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseLunaAPIProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseLunaAPIProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna API
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateLunaAPI")]
        public async Task<IActionResult> CreateLunaAPI(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateLunaAPI));

                try
                {
                    if (await IsAPIExist(appName, apiName))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.API_ALREADY_EXIST, apiName, appName));
                    }

                    var api = await _requestParser.ParseAndValidateLunaAPIAsync(await HttpUtils.GetRequestBodyAsync(req));
                    var ev = _eventGenerator.GenerateCreateLunaAPIEventContent(appName, apiName, api);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = appName,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.CreateLunaAPI.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.LunaAPIs.Add(new LunaAPIDB(appName, apiName, api.Type));
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();

                        transaction.Commit();
                    }

                    return new OkObjectResult(api);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateLunaAPI));
                }
            }
        }

        /// <summary>
        /// Update an API in the specified Luna application
        /// </summary>
        /// <group>API</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/apis/{apiName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of API</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseLunaAPIProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseLunaAPIProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna API
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseLunaAPIProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseLunaAPIProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna API
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateLunaAPI")]
        public async Task<IActionResult> UpdateLunaAPI(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateLunaAPI));

                try
                {
                    var apiToUpdate = await _dbContext.LunaAPIs.FirstOrDefaultAsync(x => x.ApplicationName == appName && x.APIName == apiName);
                    if (apiToUpdate == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.API_DOES_NOT_EXIST, apiName, appName));
                    }

                    var api = await _requestParser.ParseAndValidateLunaAPIAsync(await HttpUtils.GetRequestBodyAsync(req));

                    if (!apiToUpdate.APIType.Equals(api.Type))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.VALUE_NOT_UPDATABLE, "Type"));
                    }

                    var ev = _eventGenerator.GenerateUpdateLunaAPIEventContent(appName, apiName, api);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = appName,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.UpdateLunaAPI.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        apiToUpdate.LastUpdatedTime = DateTime.UtcNow;
                        _dbContext.LunaAPIs.Update(apiToUpdate);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();

                        transaction.Commit();
                    }

                    return new OkObjectResult(api);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateLunaAPI));
                }
            }
        }

        /// <summary>
        /// Delete an API from the specified Luna application
        /// </summary>
        /// <group>API</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/apis/{apiName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of API</param>
        /// <param name="req">The http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteLunaAPI")]
        public async Task<IActionResult> DeleteLunaAPI(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "applications/{appName}/apis/{apiName}")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteLunaAPI));

                try
                {
                    var apiToDelete = await _dbContext.LunaAPIs.FirstOrDefaultAsync(x => x.ApplicationName == appName && x.APIName == apiName);
                    if (apiToDelete == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.API_DOES_NOT_EXIST, apiName, appName));
                    }

                    if (await IsAnyAPIVersionExist(appName, apiName))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.CAN_NOT_DELETE_API_WITH_VERSIONS, apiName));
                    }

                    var ev = _eventGenerator.GenerateDeleteLunaAPIEventContent(appName, apiName);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = appName,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.DeleteLunaAPI.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.LunaAPIs.Remove(apiToDelete);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();

                        transaction.Commit();
                    }

                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteLunaAPI));
                }
            }
        }
        #endregion

        #region Luna API Version operations

        /// <summary>
        /// List all versions in specified Luna API
        /// </summary>
        /// <group>API Version</group>
        /// <verb>GET</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/apis/{apiName}/versions</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of Luna API</param>
        /// <param name="req">The http request</param>
        /// <response code="200">
        ///     <see cref="List{T}"/>
        ///     where T is <see cref="LunaAPIVersion"/>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("ListLunaAPIVersions")]
        public async Task<IActionResult> ListLunaAPIVersions(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "applications/{appName}/apis/{apiName}/versions")] HttpRequest req,
            string appName,
            string apiName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.ListLunaAPIVersions));

                try
                {
                    var versions = await _dbContext.LunaAPIVersions.
                        Select(x => x.GetAPIVersion()).
                        ToListAsync();

                    return new OkObjectResult(versions);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.ListLunaAPIVersions));
                }
            }
        }

        /// <summary>
        /// Create a version in the specified API
        /// </summary>
        /// <group>API Version</group>
        /// <verb>PUT</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/apis/{apiName}/versions/{versionName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of API</param>
        /// <param name="versionName" required="true" cref="string" in="path">Name of API version</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseAPIVersionProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseAPIVersionProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna API version
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseAPIVersionProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseAPIVersionProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna API version
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("CreateLunaAPIVersion")]
        public async Task<IActionResult> CreateLunaAPIVersion(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "applications/{appName}/apis/{apiName}/versions/{versionName}")] HttpRequest req,
            string appName,
            string apiName,
            string versionName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.CreateLunaAPIVersion));

                try
                {
                    if (!await IsApplicationExist(appName))
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, appName));
                    }

                    var api = await _dbContext.LunaAPIs.SingleOrDefaultAsync(x => x.ApplicationName == appName && x.APIName == apiName);

                    if (api == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.API_DOES_NOT_EXIST, apiName, appName));
                    }

                    if (await IsAPIVersionExist(appName, apiName, versionName))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.API_VERSION_ALREADY_EXIST, versionName, apiName));
                    }


                    var version = await _requestParser.ParseAndValidateAPIVersionAsync(await HttpUtils.GetRequestBodyAsync(req), api.APIType);
                    var ev = _eventGenerator.GenerateCreateLunaAPIVersionEventContent(appName, apiName, versionName, version);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = appName,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.CreateLunaAPIVersion.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.LunaAPIVersions.Add(new LunaAPIVersionDB(appName, apiName, versionName, version.Type));
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();

                        transaction.Commit();
                    }

                    return new OkObjectResult(version);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.CreateLunaAPIVersion));
                }
            }
        }

        /// <summary>
        /// Update a version in the specified API
        /// </summary>
        /// <group>API Version</group>
        /// <verb>PATCH</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/apis/{apiName}/versions/{versionName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of API</param>
        /// <param name="versionName" required="true" cref="string" in="path">Name of API version</param>
        /// <param name="req" in="body">
        ///     <see cref="BaseAPIVersionProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseAPIVersionProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna API version
        ///         </summary>
        ///     </example>
        ///     Request contract
        /// </param>
        /// <response code="200">
        ///     <see cref="BaseAPIVersionProp"/>
        ///     <example>
        ///         <value>
        ///             <see cref="BaseAPIVersionProp.example"/>
        ///         </value>
        ///         <summary>
        ///             An example of Luna API version
        ///         </summary>
        ///     </example>
        ///     Success
        /// </response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("UpdateLunaAPIVersion")]
        public async Task<IActionResult> UpdateLunaAPIVersion(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "applications/{appName}/apis/{apiName}/versions/{versionName}")] HttpRequest req,
            string appName,
            string apiName,
            string versionName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.UpdateLunaAPIVersion));

                try
                {
                    if (!await IsApplicationExist(appName))
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, appName));
                    }

                    var api = await _dbContext.LunaAPIs.SingleOrDefaultAsync(x => x.ApplicationName == appName && x.APIName == apiName);

                    if (api == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.API_DOES_NOT_EXIST, apiName, appName));
                    }

                    var apiVersionToUpdate = await _dbContext.LunaAPIVersions.SingleOrDefaultAsync(x => x.ApplicationName == appName && x.APIName == apiName && x.VersionName == versionName);

                    if (apiVersionToUpdate == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.API_VERSION_DOES_NOT_EXIST, versionName, apiName));
                    }

                    var version = await _requestParser.ParseAndValidateAPIVersionAsync(await HttpUtils.GetRequestBodyAsync(req), api.APIType);

                    if (!apiVersionToUpdate.VersionType.Equals(version.Type))
                    {
                        throw new LunaConflictUserException(string.Format(ErrorMessages.VALUE_NOT_UPDATABLE, "Type"));
                    }

                    var ev = _eventGenerator.GenerateUpdateLunaAPIVersionEventContent(appName, apiName, versionName, version);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = appName,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.UpdateLunaAPIVersion.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        apiVersionToUpdate.LastUpdatedTime = DateTime.UtcNow;
                        _dbContext.LunaAPIVersions.Update(apiVersionToUpdate);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();

                        transaction.Commit();
                    }

                    return new OkObjectResult(version);
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.UpdateLunaAPIVersion));
                }
            }
        }

        /// <summary>
        /// Delete a version from the specified API
        /// </summary>
        /// <group>s</group>
        /// <verb>DELETE</verb>
        /// <url>http://localhost:7071/api/applications/{appName}/apis/{apiName}/versions/{versionName}</url>
        /// <param name="appName" required="true" cref="string" in="path">Name of Luna application</param>
        /// <param name="apiName" required="true" cref="string" in="path">Name of API</param>
        /// <param name="versionName" required="true" cref="string" in="path">Name of API version</param>
        /// <param name="req">Http request</param>
        /// <response code="204">Success</response>
        /// <security type="apiKey" name="x-functions-key">
        ///     <description>Azure function key</description>
        ///     <in>header</in>
        /// </security>
        /// <returns></returns>
        [FunctionName("DeleteLunaAPIVersion")]
        public async Task<IActionResult> DeleteLunaAPIVersion(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "applications/{appName}/apis/{apiName}/versions/{versionName}")] HttpRequest req,
            string appName,
            string apiName,
            string versionName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            using (_logger.BeginManagementNamedScope(lunaHeaders))
            {
                _logger.LogMethodBegin(nameof(this.DeleteLunaAPIVersion));

                try
                {
                    if (!await IsApplicationExist(appName))
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.APPLICATION_DOES_NOT_EXIST, appName));
                    }

                    var api = await _dbContext.LunaAPIs.SingleOrDefaultAsync(x => x.ApplicationName == appName && x.APIName == apiName);

                    if (api == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.API_DOES_NOT_EXIST, apiName, appName));
                    }

                    var apiVersionToDelete = await _dbContext.LunaAPIVersions.SingleOrDefaultAsync(x => x.ApplicationName == appName && x.APIName == apiName && x.VersionName == versionName);

                    if (apiVersionToDelete == null)
                    {
                        throw new LunaNotFoundUserException(string.Format(ErrorMessages.API_VERSION_DOES_NOT_EXIST, versionName, apiName));
                    }

                    var ev = _eventGenerator.GenerateDeleteLunaAPIVersionEventContent(appName, apiName, versionName);

                    var publishingEvent = new PublishingEventDB()
                    {
                        ResourceName = appName,
                        EventId = Guid.NewGuid(),
                        EventType = PublishingEventType.DeleteLunaAPIVersion.ToString(),
                        EventContent = ev,
                        CreatedBy = lunaHeaders.UserId,
                        Tags = "",
                        CreatedTime = DateTime.UtcNow
                    };

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        _dbContext.LunaAPIVersions.Remove(apiVersionToDelete);
                        await _dbContext._SaveChangesAsync();

                        _dbContext.PublishingEvents.Add(publishingEvent);
                        await _dbContext._SaveChangesAsync();

                        transaction.Commit();
                    }

                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
                }
                finally
                {
                    _logger.LogMethodEnd(nameof(this.DeleteLunaAPIVersion));
                }
            }
        }
        #endregion

        #region Private methods

        private async Task<ApplicationSnapshotDB> CreateSnapshot(string appName,
            ApplicationStatus status,
            PublishingEventDB currentEvent = null,
            string tags = "",
            bool isNewApp = false)
        {
            var snapshot = isNewApp ? null : _dbContext.ApplicationSnapshots.
                Where(x => x.ApplicationName == appName).
                OrderByDescending(x => x.LastAppliedEventId).FirstOrDefault();

            var events = new List<BaseLunaPublishingEvent>();

            if (!isNewApp)
            {
                events = await _dbContext.PublishingEvents.
                    Where(x => x.Id > snapshot.LastAppliedEventId).
                    OrderBy(x => x.Id).
                    Select(x => x.GetEventObject()).
                    ToListAsync();
            }

            if (currentEvent != null)
            {
                events.Add(currentEvent.GetEventObject());
            }

            var newSnapshot = new ApplicationSnapshotDB()
            {
                SnapshotId = Guid.NewGuid(),
                ApplicationName = appName,
                SnapshotContent = _eventProcessor.GetLunaApplicationJSONString(appName, events, snapshot),
                Status = status.ToString(),
                Tags = "",
                CreatedTime = DateTime.UtcNow,
                DeletedTime = null
            };

            return newSnapshot;
        }

        /// <summary>
        /// Check if the Luna application exsits
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <returns>True if application exists, False otherwise</returns>
        private async Task<bool> IsApplicationExist(string appName)
        {
            return await _dbContext.LunaApplications.AnyAsync(x => x.ApplicationName == appName);
        }

        /// <summary>
        /// Check if any Luna API exsits in the specified application
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <returns>True if any API exists, False otherwise</returns>
        private async Task<bool> IsAnyAPIExist(string appName)
        {
            return await _dbContext.LunaAPIs.AnyAsync(x => x.ApplicationName == appName);
        }

        /// <summary>
        /// Check if the Luna API exsits
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <returns>True if API exists, False otherwise</returns>
        private async Task<bool> IsAPIExist(string appName, string apiName)
        {
            return await _dbContext.LunaAPIs.AnyAsync(x => x.ApplicationName == appName && x.APIName == apiName);
        }

        /// <summary>
        /// Get the API type if the API exist
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <returns>The API type if the API exist, null otherwise</returns>
        private async Task<string> GetAPITypeIfExist(string appName, string apiName)
        {
            return await _dbContext.LunaAPIs.
                Where(x => x.ApplicationName == appName && x.APIName == apiName).
                Select(x => x.APIType).
                FirstOrDefaultAsync();
        }

        /// <summary>
        /// Check if the API version exist
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The api name</param>
        /// <param name="versionName">The version name</param>
        /// <returns>True if the version exist, False otherwise</returns>
        private async Task<bool> IsAPIVersionExist(string appName, string apiName, string versionName)
        {
            return await _dbContext.LunaAPIVersions.AnyAsync(x => x.ApplicationName == appName && x.APIName == apiName && x.VersionName == versionName);
        }

        /// <summary>
        /// Check if any API version exist in the specified API
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The api name</param>
        /// <returns>True if any version exist, False otherwise</returns>
        private async Task<bool> IsAnyAPIVersionExist(string appName, string apiName)
        {
            return await _dbContext.LunaAPIVersions.AnyAsync(x => x.ApplicationName == appName && x.APIName == apiName);
        }

        #endregion
    }
}
