using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class PublishedLunaApplication
    {
        public static string example = JsonConvert.SerializeObject(new PublishedLunaApplication()
        {
            UniqueName = "mylunaapp",
            DisplayName = "My Luna App",
            Description = "This is my Luna app",
            LogoImageUrl = "https://aka.ms/lunalog.png",
            DocumentationUrl = "https://aka.ms/lunadoc",
            Publisher = "Microsoft",
            Details = new LunaApplicationDetails()
        });

        public PublishedLunaApplication()
        {
            Tags = new List<LunaPublishedApplicationTag>();
        }

        [JsonProperty(PropertyName = "UniqueName", Required = Required.Always)]
        public string UniqueName { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "LogoImageUrl", Required = Required.Default)]
        public string LogoImageUrl { get; set; }

        [JsonProperty(PropertyName = "DocumentationUrl", Required = Required.Default)]
        public string DocumentationUrl { get; set; }

        [JsonProperty(PropertyName = "Publisher", Required = Required.Always)]
        public string Publisher { get; set; }

        [JsonProperty(PropertyName = "Tags", Required = Required.Always)]
        public List<LunaPublishedApplicationTag> Tags { get; set; }

        [JsonProperty(PropertyName = "Details", Required = Required.Default)]
        public LunaApplicationDetails Details { get; set; }
    }
}
