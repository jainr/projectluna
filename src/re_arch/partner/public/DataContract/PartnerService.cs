using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luna.Partner.Public.Client
{
    public class PartnerServiceOutlineResponse
    {
        public static string example = JsonConvert.SerializeObject(new PartnerServiceOutlineResponse()
        {
            UniqueName = "amlworkspace",
            DisplayName = "My AML workspace",
            Type = "AzureML",
            Description = "This is my AML workspace",
            Tags = "department=hr",
            CreatedTime = new DateTime(637589448301320267),
            LastUpdatedTime = new DateTime(637589448301320267)
        });

        [JsonProperty(PropertyName = "UniqueName", Required = Required.Always)]
        public string UniqueName { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Tags", Required = Required.Default)]
        public string Tags { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "LastUpdatedTime", Required = Required.Default)]
        public DateTime LastUpdatedTime { get; set; }

    }
}
