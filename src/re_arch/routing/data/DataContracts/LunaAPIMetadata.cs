using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Routing.Data
{
    public class LunaAPIMetadata
    {
        public LunaAPIMetadata(string appName, string apiName, string type)
        {
            this.ApplciationName = appName;
            this.APIName = apiName;
            this.APIType = type;
            this.Versions = new List<LunaAPIVersionMetadata>();
        }

        public string ApplciationName { get; set; }
        public string APIName { get; set; }
        public string APIType { get; set; }


        public List<LunaAPIVersionMetadata> Versions { get; set; } 
    }

    public class LunaAPIVersionMetadata
    {
        public LunaAPIVersionMetadata(string name)
        {
            this.Name = name;
            this.Operations = new List<string>();
        }

        public string Name { get; set; }

        public List<string> Operations { get; set; }

    }
}
