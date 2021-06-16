using System.Collections.Generic;
using Newtonsoft.Json;

namespace Luna.Publish.Public.Client
{
    public class LunaApplication
    {

        public static string example = "{}";

        public LunaApplication()
        {
            APIs = new List<LunaAPI>();
        }

        public LunaApplication(string name, ApplicationStatus status, LunaApplicationProp properties) :
            this(name, status.ToString(), properties)
        {
        }

        public LunaApplication(string name, string status, LunaApplicationProp properties) :
            this()
        {
            this.Name = name;
            this.Status = status;
            this.Properties = properties;
        }

        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "Properties", Required = Required.Default)]
        public LunaApplicationProp Properties { get; set; }

        [JsonProperty(PropertyName = "APIs", Required = Required.Always)]
        public List<LunaAPI> APIs { get; set; }
    }
}
