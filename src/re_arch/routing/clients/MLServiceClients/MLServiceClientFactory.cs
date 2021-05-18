using Luna.Common.Utils.Azure.AzureKeyvaultUtils;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Luna.Publish.PublicClient.DataContract.APIVersions;
using Luna.Publish.PublicClient.Enums;
using Luna.Routing.Clients.MLServiceClients.Interfaces;
using Luna.Routing.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients.MLServiceClients
{
    public class MLServiceClientFactory : IMLServiceClientFactory
    {
        private static Dictionary<string, AzureMLClient> _cachedAzureMLClients = new Dictionary<string, AzureMLClient>();
        private static Dictionary<string, AzureSynapseClient> _cachedAzureSynapseClients = new Dictionary<string, AzureSynapseClient>();

        private readonly ISqlDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;

        [ActivatorUtilitiesConstructor]
        public MLServiceClientFactory(HttpClient httpClient, ISqlDbContext dbContext,
            IAzureKeyVaultUtils keyVaultUtils)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
        }

        /// <summary>
        /// Get the realtime endpoint client
        /// </summary>
        /// <param name="versionType">The api version type</param>
        /// <param name="versionProperties">The api version properties</param>
        /// <returns>The realtime endpoint client</returns>
        public async Task<IRealtimeEndpointClient> GetRealtimeEndpointClient(string versionType, BaseAPIVersionProp versionProperties)
        {
            if (versionProperties.Type.Equals(RealtimeEndpointAPIVersionType.AzureML.ToString()))
            {
                var prop = (AzureMLRealtimeEndpointAPIVersionProp)versionProperties;
                if (!_cachedAzureMLClients.ContainsKey(prop.AzureMLWorkspaceName))
                {
                    var partnerService = await _dbContext.PartnerServices.SingleOrDefaultAsync(x => x.UniqueName == prop.AzureMLWorkspaceName);
                    if (partnerService == null)
                    {
                        throw new LunaServerException($"Can not find partner service {prop.AzureMLWorkspaceName} in the view.");
                    }

                    var config = await _keyVaultUtils.GetSecretAsync(partnerService.ConfigurationSecretName);
                    var amlConfig = JsonConvert.DeserializeObject<AzureMLWorkspaceConfiguration>(config);
                    _cachedAzureMLClients.Add(prop.AzureMLWorkspaceName,
                        new AzureMLClient(this._httpClient, amlConfig));
                }

                return _cachedAzureMLClients[prop.AzureMLWorkspaceName];
            }
            return null;
        }

        /// <summary>
        /// Get the pipeline endpoint client
        /// </summary>
        /// <param name="versionType">The api version type</param>
        /// <param name="versionProperties">The api version properties</param>
        /// <returns>The pipeline endpoint client</returns>
        public async Task<IPipelineEndpointClient> GetPipelineEndpointClient(string versionType, BaseAPIVersionProp versionProperties)
        {
            if (versionProperties.Type.Equals(RealtimeEndpointAPIVersionType.AzureML.ToString()))
            {
                var prop = (AzureMLPipelineEndpointAPIVersionProp)versionProperties;

                if (!_cachedAzureMLClients.ContainsKey(prop.AzureMLWorkspaceName))
                {
                    var partnerService = await _dbContext.PartnerServices.SingleOrDefaultAsync(x => x.UniqueName == prop.AzureMLWorkspaceName);
                    if (partnerService == null)
                    {
                        throw new LunaServerException($"Can not find partner service {prop.AzureMLWorkspaceName} in the view.");
                    }

                    var config = await _keyVaultUtils.GetSecretAsync(partnerService.ConfigurationSecretName);
                    var amlConfig = JsonConvert.DeserializeObject<AzureMLWorkspaceConfiguration>(config);
                    _cachedAzureMLClients.Add(prop.AzureMLWorkspaceName,
                        new AzureMLClient(this._httpClient, amlConfig));
                }

                return _cachedAzureMLClients[prop.AzureMLWorkspaceName];
            }

            return null;
        }
    
    }
}
