using Luna.Publish.PublicClient.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.PublicClient.DataContract
{
    public class RealtimeEndpoint : BaseMLComponent
    {
        public RealtimeEndpoint(string id, string displayName) :
            base(id, displayName, LunaAPIType.Realtime)
        {

        }
    }
}
