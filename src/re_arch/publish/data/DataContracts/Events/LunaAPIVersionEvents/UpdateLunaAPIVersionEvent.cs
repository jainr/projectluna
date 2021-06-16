﻿using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public class UpdateLunaAPIVersionEvent : BaseLunaAPIVersionEvent
    {
        public UpdateLunaAPIVersionEvent()
            : base(PublishingEventType.UpdateLunaAPIVersion)
        {

        }

        public BaseAPIVersionProp Properties { get; set; }
    }
}
