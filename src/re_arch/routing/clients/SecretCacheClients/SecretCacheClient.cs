using Luna.Common.Utils;
using Luna.Routing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients
{
    public class SecretItemCache
    {
        public string SecretName { get; set; }

        public string SecretValue { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }

    public class SecretCache
    {
        public SecretCache()
        {
            SubscriptionKeys = new Dictionary<string, SecretItemCache>();
            ApplicationMasterKeys = new Dictionary<string, SecretItemCache>();
            PartnerServiceSecrets = new Dictionary<string, SecretItemCache>();
            IsInitialized = false;
        }

        public bool IsInitialized { get; set; }

        // We need to retrive by value for subscription and master keys
        public void AddSubscriptionKey(string name, string value)
        {
            if (!SubscriptionKeys.ContainsKey(value))
            {
                SubscriptionKeys.Add(value, new SecretItemCache()
                {
                    SecretName = name,
                    SecretValue = value,
                    LastUpdatedTime = DateTime.UtcNow
                });
            }
        }

        // We need to retrive by value for subscription and master keys
        public void AddApplicationMasterKey(string name, string value)
        {
            if (!ApplicationMasterKeys.ContainsKey(value))
            {
                ApplicationMasterKeys.Add(value, new SecretItemCache()
                {
                    SecretName = name,
                    SecretValue = value,
                    LastUpdatedTime = DateTime.UtcNow
                });
            }
        }

        //retrive by name for partner service secrets
        public void AddPartnerServiceSecret(string name, string value)
        {
            if (!PartnerServiceSecrets.ContainsKey(name))
            {
                PartnerServiceSecrets.Add(name, new SecretItemCache()
                {
                    SecretName = name,
                    SecretValue = value,
                    LastUpdatedTime = DateTime.UtcNow
                });
            }
        }

        public Dictionary<string, SecretItemCache> SubscriptionKeys { get; set; }
        public Dictionary<string, SecretItemCache> ApplicationMasterKeys { get; set; }
        public Dictionary<string, SecretItemCache> PartnerServiceSecrets { get; set; }
    }

    public class SecretCacheClient : ISecretCacheClient
    {
        private static SecretCache _secretCache = new SecretCache();

        private readonly ILogger<SecretCacheClient> _logger;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;

        public SecretCacheClient(
            ILogger<SecretCacheClient> logger,
            IAzureKeyVaultUtils keyVaultUtils)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
        }

        public SecretCache SecretCache
        {
            get { return _secretCache; }
        }

        private async Task InitSubscriptionKeys(List<SubscriptionsDBView> subscriptions)
        {
            foreach (var sub in subscriptions)
            {
                _secretCache.AddSubscriptionKey(
                    sub.PrimaryKeySecretName,
                    await _keyVaultUtils.GetSecretAsync(sub.PrimaryKeySecretName));

                _secretCache.AddSubscriptionKey(
                    sub.SecondaryKeySecretName,
                    await _keyVaultUtils.GetSecretAsync(sub.SecondaryKeySecretName));
            }
        }

        private async Task InitApplicationMasterKeys(List<PublishedAPIVersionDB> apiVersions)
        {
            foreach (var version in apiVersions)
            {
                _secretCache.AddApplicationMasterKey(
                    version.PrimaryMasterKeySecretName,
                    await _keyVaultUtils.GetSecretAsync(version.PrimaryMasterKeySecretName));

                _secretCache.AddApplicationMasterKey(
                    version.SecondaryMasterKeySecretName,
                    await _keyVaultUtils.GetSecretAsync(version.SecondaryMasterKeySecretName));
            }
        }

        private async Task InitPartnerServiceSecrets(List<PartnerServiceDbView> partnerServices)
        {
            foreach (var srv in partnerServices)
            {
                _secretCache.AddPartnerServiceSecret(
                    srv.ConfigurationSecretName,
                    await _keyVaultUtils.GetSecretAsync(srv.ConfigurationSecretName));
            }
        }

        public async Task RefreshApplicationMasterKey(string secretName)
        {
            var secretItem = _secretCache.ApplicationMasterKeys.
                   Where(x => x.Value.SecretName == secretName).SingleOrDefault();

            if (!secretItem.Equals(default(KeyValuePair<string, SecretItemCache>)))
            {
                _secretCache.ApplicationMasterKeys.Remove(secretItem.Key);
            }

            var secretValue = await _keyVaultUtils.GetSecretAsync(secretName);

            _secretCache.AddApplicationMasterKey(secretName, secretValue);
        }

        public async Task RefreshSubscriptionKey(string secretName)
        {
            var secretItem = _secretCache.SubscriptionKeys.
                   Where(x => x.Value.SecretName == secretName).SingleOrDefault();

            if (!secretItem.Equals(default(KeyValuePair<string, SecretItemCache>)))
            {
                _secretCache.SubscriptionKeys.Remove(secretItem.Key);
            }

            var secretValue = await _keyVaultUtils.GetSecretAsync(secretName);

            _secretCache.AddSubscriptionKey(secretName, secretValue);
        }

        public async Task RefreshPartnerServiceSecret(string secretName)
        {
            var secretItem = _secretCache.PartnerServiceSecrets.
                   Where(x => x.Value.SecretName == secretName).SingleOrDefault();

            if (!secretItem.Equals(default(KeyValuePair<string, SecretItemCache>)))
            {
                _secretCache.PartnerServiceSecrets.Remove(secretItem.Key);
            }

            var secretValue = await _keyVaultUtils.GetSecretAsync(secretName);

            _secretCache.AddPartnerServiceSecret(secretName, secretValue);
        }

        public async Task Init(List<SubscriptionsDBView> subscriptions, 
            List<PublishedAPIVersionDB> apiVersions, 
            List<PartnerServiceDbView> partnerServices)
        {
            // Initialize the secret cache in memory
            // List secrets was identified as a dangrous operation in Key vault. We will avoid that operation
            if (!_secretCache.IsInitialized)
            {
                await InitApplicationMasterKeys(apiVersions);
                await InitSubscriptionKeys(subscriptions);
                await InitPartnerServiceSecrets(partnerServices);

                _secretCache.IsInitialized = true; 
            }
        }

    }
}
