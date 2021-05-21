using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client.DataContract
{
    public class LunaAPI
    {
        public LunaAPI(string name, BaseLunaAPIProp properties = null)
        {
            this.Name = name;
            this.Properties = properties;
            Versions = new List<APIVersion>();
        }

        public string Name { get; set; }

        public BaseLunaAPIProp Properties { get; set; }

        public List<APIVersion> Versions {get;set;}
    }
}
