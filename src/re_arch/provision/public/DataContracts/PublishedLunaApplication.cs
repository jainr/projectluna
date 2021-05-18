using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Provision.Public.Client.DataContracts
{
    public class LunaApplicationSwagger
    {
        public LunaApplicationSwagger()
        {
        }

        public string UniqueName { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

    }
}
