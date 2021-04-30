using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Luna.Routing.Data.Entities;
using Luna.Routing.Clients.MLServiceClients;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.LoggingUtils;
using Luna.Common.Utils.HttpUtils;
using Luna.Common.Utils.RestClients;
using Luna.Publish.PublicClient.Enums;
using System.Web.Http;
using Luna.Publish.PublicClient.DataContract.APIVersions;
using Luna.Publish.PublicClient.DataContract.LunaApplications;
using System.Collections.Generic;
using Luna.Common.Utils.Azure.AzureStorageUtils;
using Luna.Common.Utils.Events;
using Luna.Common.Utils.Azure.AzureKeyvaultUtils;
using Luna.Common.Utils.LoggingUtils;

namespace Luna.Routing.Functions
{
    /// <summary>
    /// The service maintains all routings
    /// </summary>
    public class RoutingServiceFunctions
    {
        private const string API_VERSION_QUERY_PARAM_NAME = "api-version";

        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<RoutingServiceFunctions> _logger;
        private readonly IMLServiceClientFactory _clientFactory;
        private readonly IAzureStorageUtils _storageUtils;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;

        public RoutingServiceFunctions(ISqlDbContext dbContext, 
            ILogger<RoutingServiceFunctions> logger, 
            IMLServiceClientFactory clientFactory,
            IAzureKeyVaultUtils keyVaultUtils,
            IAzureStorageUtils storageUtils)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(dbContext));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(dbContext));
            this._storageUtils = storageUtils ?? throw new ArgumentNullException(nameof(storageUtils));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
        }

        [FunctionName("ProcessApplicationEvents")]
        public async Task ProcessApplicationEvents([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
        {
            // Get the last applied event id
            // If there's no record in the database, it will return the default value of long type 0
            var lastAppliedEventId = await _dbContext.PublishedAPIVersions.
                OrderByDescending(x => x.LastAppliedEventId).
                Select(x => x.LastAppliedEventId).FirstOrDefaultAsync();

            var events = await _storageUtils.RetrieveSortedTableEntities("ApplicationEvents", lastAppliedEventId);

            foreach (var ev in events)
            {
                if (ev.EventType.Equals(LunaEventType.PUBLISH_APPLICATION_EVENT))
                {
                    await CreateOrUpdateLunaApplication(ev.EventContent, ev.EventSequenceId);
                }
                else if (ev.EventType.Equals(LunaEventType.DELETE_APPLICATION_EVENT))
                {
                    // TODO: should not use partition key
                    await DeleteLunaApplication(ev.PartitionKey, ev.EventSequenceId);
                }
            }
        }

        /// <summary>
        /// Call a real time endpoint
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [FunctionName("CallRealtimeEndpoint")]
        public async Task<IActionResult> CallRealtimeEndpoint(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{appName}/{apiName}/{operationName}")] HttpRequest req,
        string appName,
        string apiName,
        string operationName)
        {
            var lunaHeaders = HttpUtils.GetLunaRequestHeaders(req);
            try
            {
                var versionName = GetAPIVersion(req);
                var apiVersion = await _dbContext.PublishedAPIVersions.SingleOrDefaultAsync(
                    x => x.IsEnabled &&
                    x.ApplicationName == appName &&
                    x.APIName == apiName &&
                    x.VersionName == versionName);

                if (apiVersion == null)
                {
                    throw new LunaNotFoundUserException(
                        string.Format(ErrorMessages.API_VERSION_DOES_NOT_EXIST, apiName, versionName));
                }

                if (lunaHeaders.LunaApplicationMasterKey == null)
                {
                    // TODO: check the subscription keys
                    // this is a temp error message
                    throw new LunaUnauthorizedUserException("The master key is required");
                }
                else
                {
                    //TODO: cache master keys
                    var key = await _keyVaultUtils.GetSecretAsync(apiVersion.PrimaryMasterKeySecretName);

                    if (!lunaHeaders.LunaApplicationMasterKey.Equals(key))
                    {
                        key = await _keyVaultUtils.GetSecretAsync(apiVersion.SecondaryMasterKeySecretName);

                        if (!lunaHeaders.LunaApplicationMasterKey.Equals(key))
                        {
                            throw new LunaUnauthorizedUserException("Incorrect key");
                        }
                    }
                }

                var versionProp = this.GetAPIVersionProperties(apiVersion.VersionProperties);

                if (apiVersion.APIType.Equals(LunaAPIType.Realtime.ToString()))
                {
                    var client = await _clientFactory.GetRealtimeEndpointClient(apiVersion.VersionType, versionProp);
                    var response = await client.CallRealtimeEndpoint(
                        operationName,
                        await HttpUtils.GetRequestBodyAsync(req),
                        versionProp,
                        lunaHeaders.GetPassThroughHeaders());

                    return new ContentResult()
                    {
                        Content = await response.Content.ReadAsStringAsync(),
                        ContentType = response.Content.Headers.ContentType.MediaType,
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                return ErrorUtils.HandleExceptions(ex, this._logger, lunaHeaders.TraceId);
            }
        }

        #region Private methods

        private BaseAPIVersionProp GetAPIVersionProperties(string versionProperties)
        {
            return JsonConvert.DeserializeObject<BaseAPIVersionProp>(versionProperties, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        private string GetAPIVersion(HttpRequest req)
        {
            if (req.Query.ContainsKey(API_VERSION_QUERY_PARAM_NAME))
            {
                return req.Query[API_VERSION_QUERY_PARAM_NAME];
            }
            else
            {
                throw new LunaBadRequestUserException(ErrorMessages.MISSING_QUERY_PARAMETER, UserErrorCode.MissingQueryParameter);
            }
        }

        private async Task DeleteLunaApplication(string name, long eventSequenceId)
        {
            var oldVersions = await _dbContext.PublishedAPIVersions.Where(x => x.ApplicationName == name).ToListAsync();
            var currentTime = DateTime.UtcNow;

            foreach (var oldVersion in oldVersions)
            {
                oldVersion.LastAppliedEventId = eventSequenceId;
                oldVersion.LastUpdatedTime = currentTime;
                oldVersion.IsEnabled = false;
            }

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                if (oldVersions.Count > 0)
                {
                    _dbContext.PublishedAPIVersions.UpdateRange(oldVersions);
                }

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            return;
        }

        private async Task CreateOrUpdateLunaApplication(string content, long eventSequenceId)
        {
            LunaApplication app = JsonConvert.DeserializeObject<LunaApplication>(content, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var oldVersions = await _dbContext.PublishedAPIVersions.Where(x => x.ApplicationName == app.Name).ToListAsync();
            var currentTime = DateTime.UtcNow;

            foreach (var oldVersion in oldVersions)
            {
                oldVersion.LastUpdatedTime = currentTime;
                oldVersion.IsEnabled = false;
            }

            var newVersions = new List<PublishedAPIVersionDB>();

            foreach (var api in app.APIs)
            {
                foreach (var version in api.Versions)
                {
                    var prop = JsonConvert.SerializeObject(version.Properties, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

                    var ver = new PublishedAPIVersionDB()
                    {
                        ApplicationName = app.Name,
                        APIName = api.Name,
                        APIType = api.Properties.Type,
                        VersionName = version.Name,
                        VersionType = version.Properties.Type,
                        VersionProperties = prop,
                        LastAppliedEventId = eventSequenceId,
                        PrimaryMasterKeySecretName = app.Properties.PrimaryMasterKeySecretName,
                        SecondaryMasterKeySecretName = app.Properties.SecondaryMasterKeySecretName,
                        IsEnabled = true,
                        CreatedTime = currentTime,
                        LastUpdatedTime = currentTime
                    };
                    newVersions.Add(ver);
                }
            }

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                if (newVersions.Count > 0)
                {
                    _dbContext.PublishedAPIVersions.AddRange(newVersions);
                }

                if (oldVersions.Count > 0)
                {
                    _dbContext.PublishedAPIVersions.UpdateRange(oldVersions);
                }

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            return;
        }
        #endregion
    }
}
