using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client.DataContracts
{
    public class LunaApplicationSubscriptionOwner
    {
        public static string example = JsonConvert.SerializeObject(new LunaApplicationSubscriptionOwner()
        {
            UserId = Guid.NewGuid().ToString(),
            UserName = "FirstName LastName"
        });

        [JsonProperty(PropertyName = "UserId", Required = Required.Always)]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "UserName", Required = Required.Default)]
        public string UserName { get; set; }
    }
}
