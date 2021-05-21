using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.PublicClient.Enums
{
    public enum RealtimeEndpointAPIVersionType
    {
        AzureML,
        AzureDatabricks
    }

    public enum PipelineEndpointAPIVersionType
    {
        AzureML
    }

    public enum MLProjectAPIVersionType
    {
        AzureML,
        AzureDatabricks,
        AzureSynapse
    }
}
