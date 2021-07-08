using Luna.Gallery.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients
{
    public interface ISyncProvisionStepClient
    {
        Task<List<MarketplaceSubscriptionParameter>> RunAsync(List<MarketplaceSubscriptionParameter> parameters);

    }
}
