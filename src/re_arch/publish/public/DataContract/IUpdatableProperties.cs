using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client.DataContract
{
    public abstract class UpdatableProperties
    {
        public abstract void Update(UpdatableProperties propertis);
    }
}
