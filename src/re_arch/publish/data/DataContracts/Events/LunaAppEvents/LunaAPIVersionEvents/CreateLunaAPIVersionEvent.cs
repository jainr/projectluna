using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class CreateLunaAPIVersionEvent : BaseLunaAPIVersionEvent
    {
        public CreateLunaAPIVersionEvent()
            : base(LunaAppEventType.CreateLunaAPIVersion)
        {

        }

        public BaseAPIVersionProp Properties { get; set; }
    }
}
