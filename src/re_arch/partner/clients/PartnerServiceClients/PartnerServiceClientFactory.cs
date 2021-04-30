using Luna.Partner.PublicClient.DataContract;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.Clients.PartnerServiceClients
{
    public class PartnerServiceClientFactory : IPartnerServiceClientFactory
    {
        private static Dictionary<string, IPartnerServiceClient> _azureMLClients;

        private static Dictionary<string, IPartnerServiceClient> _azureSynapseClients;
        
        public PartnerServiceClientFactory()
        {
            _azureMLClients = new Dictionary<string, IPartnerServiceClient>();
            _azureSynapseClients = new Dictionary<string, IPartnerServiceClient>();
        }

        /// <summary>
        /// Get or create a partner service client
        /// </summary>
        /// <param name="name">The partner service name</param>
        /// <param name="config">The partner service config</param>
        /// <returns>The partner service client</returns>
        public IPartnerServiceClient GetPartnerServiceClient(string name, BasePartnerServiceConfiguration config)
        {
            if (config.Type.Equals(PartnerServiceType.AML.ToString(), 
                StringComparison.InvariantCultureIgnoreCase))
            {
                if (_azureMLClients.ContainsKey(name))
                {
                    _azureMLClients[name].UpdateConfiguration(config);
                    return _azureMLClients[name];
                }
                else
                {
                    IPartnerServiceClient client = new AzureMLWorkspaceClient(config);
                    _azureMLClients.TryAdd(name, client);
                    return client;
                }
            }

            return null;
        }
    }
}
