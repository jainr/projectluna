using Luna.Common.Utils;
using Luna.Routing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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

    }

    public class SecretCache
    {
        public SecretCache()
        {
            SubscriptionKeys = new ConcurrentDictionary<string, SecretItemCache>();
            ApplicationMasterKeys = new ConcurrentDictionary<string, SecretItemCache>();
            SubscriptionKeysLastRefreshedEventId = 0;
            ApplicationMasterKeysLastRefreshedEventId = 0;
        }

        public long SubscriptionKeysLastRefreshedEventId { get; set; }

        public long ApplicationMasterKeysLastRefreshedEventId { get; set; }

        public ConcurrentDictionary<string, SecretItemCache> SubscriptionKeys { get; set; }

        public ConcurrentDictionary<string, SecretItemCache> ApplicationMasterKeys { get; set; }
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

        private async Task RefreshCachedSecretAsync(ConcurrentDictionary<string, SecretItemCache> cache, string secretName)
        {
            var secretItem = cache.Where(x => x.Value.SecretName == secretName).SingleOrDefault();

            if (!secretItem.Equals(default(KeyValuePair<string, SecretItemCache>)))
            {
                SecretItemCache value;
                cache.TryRemove(secretItem.Key, out value);
            }

            var secretValue = await _keyVaultUtils.GetSecretAsync(secretName);

            cache.TryAdd(secretValue, new SecretItemCache()
            {
                SecretName = secretName,
                SecretValue = secretValue
            });
        }

        public async Task UpdateSecretCacheAsync(List<LunaApplicationSubscriptionDB> subscriptions, List<PublishedAPIVersionDB> applications)
        {

            foreach(var sub in subscriptions)
            {
                await RefreshCachedSecretAsync(_secretCache.SubscriptionKeys, sub.PrimaryKeySecretName);
                await RefreshCachedSecretAsync(_secretCache.SubscriptionKeys, sub.SecondaryKeySecretName);
                _secretCache.SubscriptionKeysLastRefreshedEventId =
                    _secretCache.SubscriptionKeysLastRefreshedEventId > sub.LastAppliedEventId ?
                    _secretCache.SubscriptionKeysLastRefreshedEventId : sub.LastAppliedEventId;
            }

            foreach (var app in applications)
            {
                await RefreshCachedSecretAsync(_secretCache.ApplicationMasterKeys, app.PrimaryMasterKeySecretName);
                await RefreshCachedSecretAsync(_secretCache.ApplicationMasterKeys, app.SecondaryMasterKeySecretName);
                _secretCache.ApplicationMasterKeysLastRefreshedEventId =
                    _secretCache.ApplicationMasterKeysLastRefreshedEventId > app.LastAppliedEventId ?
                    _secretCache.ApplicationMasterKeysLastRefreshedEventId : app.LastAppliedEventId;
            }
        }

    }
}
