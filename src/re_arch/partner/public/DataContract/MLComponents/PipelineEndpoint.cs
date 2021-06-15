using Luna.Publish.PublicClient.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.Public.Client
{
    public class PipelineEndpoint : BaseMLComponent
    {
        public PipelineEndpoint(string id, string name) :
            base(id, name, LunaAPIType.Pipeline)
        {

        }
    }
}
