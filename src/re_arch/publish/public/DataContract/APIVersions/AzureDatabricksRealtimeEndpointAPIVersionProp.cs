using System.Collections.Generic;

namespace Luna.Publish.Public.Client
{
    public class AzureDatabricksRealtimeEndpoint
    {
        public string EndpointName { get; set; }

        public string OperationName { get; set; }

        public string EndpointVersion { get; set; }

        public string Description { get; set; }
    }

    public class AzureDatabricksRealtimeEndpointAPIVersionProp : RealtimeEndpointAPIVersionProp
    {
        public AzureDatabricksRealtimeEndpointAPIVersionProp() : 
            base(RealtimeEndpointAPIVersionType.AzureDatabricks)
        {
            Endpoints = new List<AzureDatabricksRealtimeEndpoint>();
        }

        public override void Update(UpdatableProperties properties)
        {
            var value = (AzureDatabricksRealtimeEndpointAPIVersionProp)properties;
            this.AzureDatabricksWorkspaceName = value.AzureDatabricksWorkspaceName ?? this.AzureDatabricksWorkspaceName;
            this.Endpoints = (value.Endpoints == null || value.Endpoints.Count == 0) ? this.Endpoints : value.Endpoints;
            base.Update(properties);
        }

        public string AzureDatabricksWorkspaceName { get; set; }

        public List<AzureDatabricksRealtimeEndpoint> Endpoints { get; set; }

    }
}
