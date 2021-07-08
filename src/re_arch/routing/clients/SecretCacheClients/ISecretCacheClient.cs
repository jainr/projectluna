using Luna.Routing.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients
{
    public interface ISecretCacheClient
    {
        SecretCache SecretCache { get; }

        Task UpdateSecretCacheAsync(List<LunaApplicationSubscriptionDB> subscriptions, List<PublishedAPIVersionDB> applications);

    }
}
