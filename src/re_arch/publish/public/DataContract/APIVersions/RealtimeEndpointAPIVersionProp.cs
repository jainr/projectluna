﻿using Luna.Publish.PublicClient.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client.DataContract
{
    public class RealtimeEndpointAPIVersionProp : BaseAPIVersionProp
    {
        public RealtimeEndpointAPIVersionProp(RealtimeEndpointAPIVersionType type)
            : base(type.ToString())
        {
        }
    }
}
