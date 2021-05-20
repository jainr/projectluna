using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client.DataContracts
{
    public class LunaApplicationSubscriptionKeys
    {
        public static string example = JsonConvert.SerializeObject(new LunaApplicationSubscriptionKeys()
        {
            PrimaryKey = Guid.NewGuid().ToString("N"),
            SecondaryKey = Guid.NewGuid().ToString("N"),
        });

        [JsonProperty(PropertyName = "PrimaryKey", Required = Required.Always)]
        public string PrimaryKey { get; set; }

        [JsonProperty(PropertyName = "SecondaryKey", Required = Required.Always)]
        public string SecondaryKey { get; set; }
    }
}
