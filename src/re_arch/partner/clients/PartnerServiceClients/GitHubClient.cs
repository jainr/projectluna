using Luna.Common.Utils;
using Luna.Partner.Public.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Clients.PartnerServiceClients
{
    public class GitHubClient : IPartnerServiceClient
    {
        private GitHubPartnerServiceConfiguration _config;
        private HttpClient _httpClient;
        private IEncryptionUtils _encryptionUtils;

        public GitHubClient(HttpClient httpClient,
            IEncryptionUtils encryptionUtils,
            BasePartnerServiceConfiguration configuration)
        {
            this._config = (GitHubPartnerServiceConfiguration)configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._encryptionUtils = encryptionUtils ?? throw new ArgumentNullException(nameof(encryptionUtils));
            Task task = this._config.DecryptSecretsAsync(encryptionUtils);
            task.Wait();
        }

        /// <summary>
        /// Validate an registered partner service
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            return true;
        }

        /// <summary>
        /// Update the configuration of the service client
        /// </summary>
        /// <param name="configuration">The configuration in JSON format</param>
        public async Task UpdateConfigurationAsync(BasePartnerServiceConfiguration configuration)
        {
            this._config = (GitHubPartnerServiceConfiguration)configuration;
            await this._config.DecryptSecretsAsync(this._encryptionUtils);
        }
    }
}
