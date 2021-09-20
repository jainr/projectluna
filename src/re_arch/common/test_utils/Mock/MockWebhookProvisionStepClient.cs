using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using Luna.Provision.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Test
{
    public class MockWebhookProvisionStepClient : BaseProvisionStepClient, ISyncProvisionStepClient
    {
        private static readonly string[] _createLunaApplicationRequiredInputParameters = { "SubscriptionId", "SubscriptionName", "OwnerId", "LunaApplicationName" };

        public WebhookProvisioningStepProp Properties { get; set; }

        public MockWebhookProvisionStepClient(WebhookProvisioningStepProp properties)
        {
            this.Properties = properties;
        }

        public async Task<List<MarketplaceSubscriptionParameter>> RunAsync(List<MarketplaceSubscriptionParameter> parameters)
        {
            if (this.Properties.WebhookUrl == null && this.Properties.WebhookAuthKey.Equals("x-functions-key"))
            {
                // This is the pre-step to create Luna applciation

                foreach(var param in _createLunaApplicationRequiredInputParameters)
                {
                    if (parameters.Where(x => x.Name == param).Count() == 0)
                    {
                        throw new LunaServerException($"Required parameter {param} is not provided!");
                    }
                }

                parameters.Add(new MarketplaceSubscriptionParameter
                {
                    Name = "BaseUrl",
                    Value = "https://routing.com",
                    Type = MarketplaceParameterValueType.String.ToString(),
                    IsSystemParameter = true
                });

                parameters.Add(new MarketplaceSubscriptionParameter
                {
                    Name = "PrimaryKey",
                    Value = "primarykey",
                    Type = MarketplaceParameterValueType.String.ToString(),
                    IsSystemParameter = true
                });

                parameters.Add(new MarketplaceSubscriptionParameter
                {
                    Name = "SecondaryKey",
                    Value = "secondarykey",
                    Type = MarketplaceParameterValueType.String.ToString(),
                    IsSystemParameter = true
                });
            }

            return parameters;
        }
    }
}
