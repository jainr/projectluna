using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Utils
{
    public class AzureKeyVaultUtils : IAzureKeyVaultUtils
    {
        private static Random random = new Random();
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private readonly KeyVaultClient _keyVaultClient;
        private readonly ILogger<AzureKeyVaultUtils> _logger;
        private readonly string _vaultBaseUrl;

        [ActivatorUtilitiesConstructor]
        public AzureKeyVaultUtils(IOptionsMonitor<AzureKeyVaultConfiguration> option, 
            HttpClient httpClient, 
            ILogger<AzureKeyVaultUtils> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            AzureServiceTokenProvider azureServiceTokenProvider = null;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("USER_ASSIGNED_MANAGED_IDENTITY")))
            {
                var connectionString = $"RunAs=App;AppId={Environment.GetEnvironmentVariable("USER_ASSIGNED_MANAGED_IDENTITY")}";
                azureServiceTokenProvider = new AzureServiceTokenProvider(connectionString: connectionString);
            }
            else
            {
                azureServiceTokenProvider = new AzureServiceTokenProvider();
            }

            _keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback
                ), httpClient
            );

            _logger.LogInformation("Initialize the key vault.");

            _vaultBaseUrl = $"https://{option.CurrentValue.KeyVaultName}.vault.azure.net/";
        }

        /// <summary>
        /// Get a random string with specified length
        /// </summary>
        /// <param name="length">The length of the random string</param>
        /// <returns></returns>
        private static string GetRandomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generate a secret name with specified prefix
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <returns></returns>
        public static string GenerateSecretName(string prefix)
        {
            return string.Format("{0}{1}", prefix, GetRandomString(12));
        }

        /// <summary>
        /// Set a secret in key vault
        /// </summary>
        /// <param name="secretName">The secret name</param>
        /// <param name="value">The secret value</param>
        /// <returns>True if the secret is set, False otherwise</returns>
        public async Task<bool> SetSecretAsync(string secretName, string value)
        {
            try
            {
                _logger.LogInformation("Set secret {0} in key vault.", secretName);
                var secret = await _keyVaultClient.SetSecretAsync(_vaultBaseUrl, secretName, value);
                _logger.LogInformation("Secret {0} set in key vault.", secretName);
                return true;
            }
            catch (Exception ex)
            {
                // DO NOT log secret value!
                throw new LunaServerException($"Can not set secret {secretName} in Azure key vault.", innerException: ex);
            }
        }

        /// <summary>
        /// Get a secret from key vault
        /// </summary>
        /// <param name="secretName">The secret name</param>
        /// <returns>The secret value as string. Null if failed to get the value.</returns>
        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                _logger.LogInformation("Get secret {0} from key vault.", secretName);
                var secret = await _keyVaultClient.GetSecretAsync(_vaultBaseUrl, secretName);

                _logger.LogInformation("Secret {0} got from key vault.", secretName);
                return secret.Value;
            }
            catch (Exception ex)
            {
                throw new LunaServerException($"Can not get secret {secretName} in Azure key vault.", innerException: ex);
            }
        }

        /// <summary>
        /// Disable all version of a secret from key vault
        /// </summary>
        /// <param name="secretName">The secret name</param>
        /// <returns>True if the secret is disabled, False otherwise</returns>
        public async Task<bool> DisableSecretAsync(string secretName)
        {
            try
            {
                _logger.LogInformation("Disable secret {0} from key vault.", secretName);
                var secret = await _keyVaultClient.GetSecretAsync(_vaultBaseUrl, secretName);
                _logger.LogInformation("Secret {0} disabled key vault.", secretName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Delete a secret from key vault
        /// </summary>
        /// <param name="secretName">The secret name</param>
        /// <returns>True if the secret is deleted, False otherwise</returns>
        public async Task<bool> DeleteSecretAsync(string secretName)
        {
            try
            {
                _logger.LogInformation("Delete secret {0} from key vault.", secretName);
                var secret = await _keyVaultClient.DeleteSecretAsync(_vaultBaseUrl, secretName);
                _logger.LogInformation("Secret {0} deleted key vault.", secretName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}
