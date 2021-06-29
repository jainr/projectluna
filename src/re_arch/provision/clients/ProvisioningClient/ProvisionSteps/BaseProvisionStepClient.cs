using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Clients
{
    public abstract class BaseProvisionStepClient
    {
        public bool IsSynchronized { get; set; }

        public string Mode { get; set; } 

    }
}
