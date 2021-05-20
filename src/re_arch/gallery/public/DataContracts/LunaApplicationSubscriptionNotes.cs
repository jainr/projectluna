using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client.DataContracts
{
    public class LunaApplicationSubscriptionNotes
    {
        public static string example = JsonConvert.SerializeObject(new LunaApplicationSubscriptionNotes()
        {
            Notes = "this is my subscription"
        });

        [JsonProperty(PropertyName = "Notes", Required = Required.Always)]
        public string Notes { get; set; }
    }
}
