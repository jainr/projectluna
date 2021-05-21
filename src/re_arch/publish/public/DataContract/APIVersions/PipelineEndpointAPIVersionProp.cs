using Luna.Publish.PublicClient.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client.DataContract
{
    public class PipelineEndpointAPIVersionProp : BaseAPIVersionProp
    {
        public PipelineEndpointAPIVersionProp(PipelineEndpointAPIVersionType type)
            :base(type.ToString())
        {
        }

    }
}
