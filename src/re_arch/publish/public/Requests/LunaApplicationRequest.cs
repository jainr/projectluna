using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public class LunaApplicationRequest
    {
        public static string example = JsonConvert.SerializeObject(new LunaApplicationRequest()
        {
            OwnerUserId = "43ebb35e-be1a-4dbf-92da-fb8a069d6a2c",
            DisplayName = "My App",
            Description = "This is my application",
            DocumentationUrl = "https://aka.ms/lunadoc",
            LogoImageUrl = "https://aka.ms/lunalogo.png",
            Publisher = "Microsoft",
            Tags = new List<LunaTagRequest>(
                new LunaTagRequest[]
                {
                    new LunaTagRequest()
                    {
                        Key = "Department",
                        Value = "HR"
                    }
                })
        });

        [JsonProperty(PropertyName = "ownerUserId", Required = Required.Default)]
        public string OwnerUserId { get; set; }

        [JsonProperty(PropertyName = "displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "documentationUrl", Required = Required.Always)]
        public string DocumentationUrl { get; set; }

        [JsonProperty(PropertyName = "logoImageUrl", Required = Required.Always)]
        public string LogoImageUrl { get; set; }

        [JsonProperty(PropertyName = "publisher", Required = Required.Always)]
        public string Publisher { get; set; }

        [JsonProperty(PropertyName = "tags", Required = Required.Always)]
        public List<LunaTagRequest> Tags { get; set; }
    }
}
