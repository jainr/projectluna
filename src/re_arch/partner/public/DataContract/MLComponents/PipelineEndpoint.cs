using Luna.Publish.PublicClient.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.PublicClient.DataContract
{
    public class PipelineEndpoint : BaseMLComponent
    {
        public PipelineEndpoint(string id, string displayName) :
            base(id, displayName, LunaAPIType.Realtime)
        {

        }
    }
}
