using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class LunaApplicationSwagger : LunaApplicationDetails
    {
        public static string example = "{}";
    }

    public class LunaAPIVersionDetails
    {

        public LunaAPIVersionDetails()
        {
            Operations = new List<string>();
        }

        public string Name { get; set; }

        public List<string> Operations { get; set; }
    }

    public class LunaAPIDetails
    {
        public LunaAPIDetails()
        {
            Versions = new List<LunaAPIVersionDetails>();
        }

        public string Name { get; set; }

        public string Type { get; set; }

        public List<LunaAPIVersionDetails> Versions { get; set; }
    }

    public class LunaApplicationDetails
    {
        public static string example = "{}";

        public LunaApplicationDetails()
        {
            APIs = new List<LunaAPIDetails>();
        }

        public List<LunaAPIDetails> APIs { get; set; }
    }

    public class LunaPublishedApplicationTag
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
