using System.Collections.Generic;

namespace Luna.Publish.Public.Client
{
    public class AzureMLPipelineEndpoint
    {
        public string EndpointId { get; set; }

        public string OperationName { get; set; }
    }

    public class AzureMLPipelineEndpointAPIVersionProp : PipelineEndpointAPIVersionProp
    {
        public AzureMLPipelineEndpointAPIVersionProp() : 
            base (PipelineEndpointAPIVersionType.AzureML)
        {
            Endpoints = new List<AzureMLPipelineEndpoint>();
        }

        public override void Update(UpdatableProperties properties)
        {
            var value = (AzureMLPipelineEndpointAPIVersionProp)properties;
            this.AzureMLWorkspaceName = value.AzureMLWorkspaceName ?? this.AzureMLWorkspaceName;
            this.Endpoints = (value.Endpoints == null || value.Endpoints.Count == 0) ? this.Endpoints : value.Endpoints;
            base.Update(properties);
        }

        public string AzureMLWorkspaceName { get; set; }

        public List<AzureMLPipelineEndpoint> Endpoints { get; set; }
    }
}
