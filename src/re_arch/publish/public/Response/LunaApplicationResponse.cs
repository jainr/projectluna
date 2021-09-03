using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public class LunaApplicationResponse
    {
        public static string example = JsonConvert.SerializeObject(new LunaApplicationResponse()
        {
            Name = "myapp",
            OwnerUserId = "43ebb35e-be1a-4dbf-92da-fb8a069d6a2c",
            DisplayName = "My App",
            Description = "This is my application",
            DocumentationUrl = "https://aka.ms/lunadoc",
            LogoImageUrl = "https://aka.ms/lunalogo.png",
            Publisher = "Microsoft",
            Tags = new List<LunaTagResponse>(
                new LunaTagResponse[]
                {
                    new LunaTagResponse()
                    {
                        Key = "Department",
                        Value = "HR"
                    }
                }),
            Status = ApplicationStatus.Published.ToString(),
            CreatedTime = DateTime.Parse("12:00:00 01/01/2021"),
            LastUpdatedTime = DateTime.Parse("12:00:00 01/01/2021"),
        });

        [JsonProperty(PropertyName = "Name", Required = Required.Default)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "OwnerUserId", Required = Required.Default)]
        public string OwnerUserId { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "DocumentationUrl", Required = Required.Always)]
        public string DocumentationUrl { get; set; }

        [JsonProperty(PropertyName = "LogoImageUrl", Required = Required.Always)]
        public string LogoImageUrl { get; set; }

        [JsonProperty(PropertyName = "Publisher", Required = Required.Always)]
        public string Publisher { get; set; }

        [JsonProperty(PropertyName = "Tags", Required = Required.Always)]
        public List<LunaTagResponse> Tags { get; set; }

        [JsonProperty(PropertyName = "Status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Always)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "LastUpdatedTime", Required = Required.Always)]
        public DateTime LastUpdatedTime { get; set; }
    }
}
