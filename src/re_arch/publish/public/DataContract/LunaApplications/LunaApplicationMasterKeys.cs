using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client.DataContract
{
    public class LunaApplicationMasterKeys
    {
        public static string example = JsonConvert.SerializeObject(new LunaApplicationMasterKeys()
        {
            PrimaryMasterKey = Guid.NewGuid().ToString("N"),
            SecondaryMasterKey = Guid.NewGuid().ToString("N"),
        });

        [JsonProperty(PropertyName = "PrimaryMasterKey", Required = Required.Always)]
        public string PrimaryMasterKey { get; set; }

        [JsonProperty(PropertyName = "SecondaryMasterKey", Required = Required.Always)]
        public string SecondaryMasterKey { get; set; }
    }
}
