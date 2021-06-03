using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;

namespace Luna.Routing.Data.DataContracts
{
    public class AzureMLRealtimeEndpointCache
    {
        public string Url { get; set; }

        public bool AuthEnabled { get; set; }

        public bool AadAuthEnabled { get; set; }

        public string Key { get; set; }
    }

    public class AzureMLPipelineEndpointCache
    {
        public string Url { get; set; }
    }

    /// <summary>
    /// The cache for machine learning components in an Azure ML workspace
    /// </summary>
    public class AzureMLCache
    {
        public AzureMLCache()
        {
            RealTimeEndpoints = new Dictionary<string, AzureMLRealtimeEndpointCache>();
            PipelineEndpoints = new Dictionary<string, AzureMLPipelineEndpointCache>();
        }

        public Dictionary<string, AzureMLRealtimeEndpointCache> RealTimeEndpoints { get; set; }

        public Dictionary<string, AzureMLPipelineEndpointCache> PipelineEndpoints { get; set; }

    }
}
