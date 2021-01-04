using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Clients.Models.Provisioning;
using Luna.Data.DataContracts.Luna.AI;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Luna.Clients
{
    /// <summary>
    /// An HTTP client for deploying resources with ARM templates and the resource manager API.
    /// </summary>
    public class AMLClient : RestClient<AMLClient>, IAMLClient
    {
        private readonly IKeyVaultHelper _keyVaultHelper;
        SecuredProvisioningClientConfiguration _options;

        [ActivatorUtilitiesConstructor]
        public AMLClient(
            IOptionsMonitor<SecuredProvisioningClientConfiguration> optionsMonitor,
            ILogger<AMLClient> logger,
            IKeyVaultHelper keyVaultHelper,
            HttpClient httpClient) : base(
            optionsMonitor.CurrentValue,
            logger,
            httpClient)
        {
            _options = optionsMonitor.CurrentValue;
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
        }

        private AuthenticationConfiguration GetAuthenticationConfiguration(AMLWorkspace workspace)
        {
            var config = new AuthenticationConfiguration();
            config.AppKey = workspace.AADApplicationSecretName;
            config.TenantId = workspace.AADTenantId;
            config.ClientId = workspace.AADApplicationId;
            config.VaultName = "";
            return config;
        }

        public async Task<List<MLModelArtifact>> GetModels(AMLWorkspace workspace)
        {
            var requestUrl = new Uri("");
            var bearerToken = await _keyVaultHelper.GetBearerToken(
                GetAuthenticationConfiguration(workspace),
                _options.ClientService.AuthenticationResourceId);

            var response = await SendRequest(
                HttpMethod.Head,
                requestUrl,
                Guid.NewGuid(),
                Guid.NewGuid(),
                bearerToken,
                null,
                null,
                CancellationToken.None
            );

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<MLModelArtifact>();
            }
            if (response.IsSuccessStatusCode)
            {
                return new List<MLModelArtifact>();
            }

            throw new LunaProvisioningException(
                $"Request failed to check if resource group exists.",
                ExceptionUtils.IsHttpErrorCodeRetryable(response.StatusCode));
        }
    }
}
