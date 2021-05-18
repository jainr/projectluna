using Luna.Routing.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients.SecretCacheClients
{
    public interface ISecretCacheClient
    {
        SecretCache SecretCache { get; }

        Task Init(List<SubscriptionsDBView> subscriptions,
            List<PublishedAPIVersionDB> apiVersions,
            List<PartnerServiceDbView> partnerServices);

        Task RefreshApplicationMasterKey(string secretName);
        Task RefreshSubscriptionKey(string secretName);
        Task RefreshPartnerServiceSecret(string secretName);
    }
}
