using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class ApplicationPublisher
    {
        public static string example = JsonConvert.SerializeObject(new ApplicationPublisher()
        {
            Name = "ace",
            DisplayName = "ACE team",
            Type = "Internal",
            Description = "Azure Customer Engineering Team",
            EndpointUrl = "https://aka.ms/aceendpoint",
            WebsiteUrl = "https://aka.ms/acewebsite",
            IsEnabled = true,
            PublisherKey = "publisher-key"
        });

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateObjectId(Name, nameof(Name));
            ValidationUtils.ValidateStringValueLength(DisplayName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(Description));
            ValidationUtils.ValidateEnum(Type, typeof(ApplicationPublisherType), nameof(Type));
            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));

            ValidationUtils.ValidateHttpsUrl(EndpointUrl, nameof(EndpointUrl));
            ValidationUtils.ValidateHttpsUrl(WebsiteUrl, nameof(WebsiteUrl));

            ValidationUtils.ValidateStringValueLength(PublisherKey, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(PublisherKey));

        }

        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "EndpointUrl", Required = Required.Default)]
        public string EndpointUrl { get; set; }

        [JsonProperty(PropertyName = "WebsiteUrl", Required = Required.Default)]
        public string WebsiteUrl { get; set; }

        [JsonProperty(PropertyName = "IsEnabled", Required = Required.Always)]
        public bool IsEnabled { get; set; }

        [JsonProperty(PropertyName = "PublisherKey", Required = Required.Default)]
        public string PublisherKey { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime? CreatedTime { get; set; }

        [JsonProperty(PropertyName = "LastUpdatedTime", Required = Required.Default)]
        public DateTime? LastUpdatedTime { get; set; }
    }
}
