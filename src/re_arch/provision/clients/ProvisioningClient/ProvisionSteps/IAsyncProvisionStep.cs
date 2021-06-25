using Luna.Gallery.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients
{
    public interface IAsyncProvisionStep
    {
        Task<List<MarketplaceSubscriptionParameter>> StartAsync(List<MarketplaceSubscriptionParameter> parameters);

        Task<ProvisionStepExecutionResult> CheckExecutionStatusAsync(List<MarketplaceSubscriptionParameter> parameters);

        Task<List<MarketplaceSubscriptionParameter>> FinishAsync(List<MarketplaceSubscriptionParameter> parameters);

    }
}
