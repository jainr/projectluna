using Luna.Partner.PublicClient.DataContract;
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
        /// <param name="service">The partner service</param>
        /// <returns>The partner service client</returns>
        public IPartnerServiceClient GetPartnerServiceClient(PartnerService service)
        {
            if (service.Type.Equals(PartnerServiceTypes.AML.ToString(), 
                StringComparison.InvariantCultureIgnoreCase))
            {
                if (_azureMLClients.ContainsKey(service.UniqueName))
                {
                    _azureMLClients[service.UniqueName].UpdateConfiguration(service.Configuration);
                    return _azureMLClients[service.UniqueName];
                }
                else
                {
                    IPartnerServiceClient client = new AzureMLWorkspaceClient(service.Configuration);
                    _azureMLClients.TryAdd(service.UniqueName, client);
                    return client;
                }
            }

            return null;
        }
    }
}
