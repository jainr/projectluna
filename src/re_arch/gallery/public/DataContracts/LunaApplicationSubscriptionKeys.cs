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
            PrimaryKey = "3cbeda43ef6b4dc0b61220fbb2f7dda4",
            SecondaryKey = "fc7cfaab23b0494aa53f178f7fbc720c",
        });

        [JsonProperty(PropertyName = "PrimaryKey", Required = Required.Always)]
        public string PrimaryKey { get; set; }

        [JsonProperty(PropertyName = "SecondaryKey", Required = Required.Always)]
        public string SecondaryKey { get; set; }
    }
}
