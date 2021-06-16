using System.Collections.Generic;

namespace Luna.Publish.Public.Client
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
