using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Data
{
    public enum ProvisionStatus
    {
        Queued,
        Running,
        Failed,
        Completed,
        Aborted
    }
}
