using Luna.Provision.Data;
using Luna.PubSub.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients
{
    public interface IProvisionFunctionsImpl
    {
        Task ProcessActiveProvisioningJobStepAsync(MarketplaceSubProvisionJobDB job);

        Task<Guid?> ActivateQueuedProvisioningJobAsync(MarketplaceSubProvisionJobDB job);

        Task ProcessMarketplaceOfferEventAsync(LunaBaseEventEntity ev);

        Task ProcessMarketplaceSubscriptionEventAsync(LunaBaseEventEntity ev);
    }
}
