using Luna.Common.Utils;
using Luna.Publish.PublicClient.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Publish.Public.Client.DataContract
{
    public class PipelineEndpointLunaAPIProp : BaseLunaAPIProp
    {
        public PipelineEndpointLunaAPIProp() :
            base(LunaAPIType.Pipeline)
        {

        }

    }
}
