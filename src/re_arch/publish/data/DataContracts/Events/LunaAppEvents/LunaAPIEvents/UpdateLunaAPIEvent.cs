using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class UpdateLunaAPIEvent : BaseLunaAPIEvent
    {
        public UpdateLunaAPIEvent()
            : base(LunaAppEventType.UpdateLunaAPI)
        {

        }

        public BaseLunaAPIProp Properties { get; set; }

    }
}
