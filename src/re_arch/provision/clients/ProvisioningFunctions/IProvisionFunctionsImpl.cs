using Luna.Provision.Data;
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
    }
}
