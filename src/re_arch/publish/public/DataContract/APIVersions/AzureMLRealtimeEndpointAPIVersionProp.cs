using System.Collections.Generic;

namespace Luna.Publish.Public.Client
{
    public class AzureMLRealtimeEndpoint
    {
        public string EndpointName { get; set; }

        public string OperationName { get; set; }

        public string Description { get; set; }
    }

    public class AzureMLRealtimeEndpointAPIVersionProp : RealtimeEndpointAPIVersionProp
    {
        public AzureMLRealtimeEndpointAPIVersionProp() : 
            base(RealtimeEndpointAPIVersionType.AzureML)
        {
            Endpoints = new List<AzureMLRealtimeEndpoint>();
        }

        public override void Update(UpdatableProperties properties)
        {
            var value = (AzureMLRealtimeEndpointAPIVersionProp)properties;
            this.AzureMLWorkspaceName = value.AzureMLWorkspaceName ?? this.AzureMLWorkspaceName;
            this.Endpoints = (value.Endpoints == null || value.Endpoints.Count == 0) ? this.Endpoints : value.Endpoints;
            base.Update(properties);
        }

        public string AzureMLWorkspaceName { get; set; }

        public List<AzureMLRealtimeEndpoint> Endpoints { get; set; }
    }
}
