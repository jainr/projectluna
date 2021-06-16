using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Luna.Publish.Public.Client
{
    public class AutomationWebhook
    {
        public static string example = JsonConvert.SerializeObject(new AutomationWebhook()
        {
            Name = "responsible-ai-scan",
            Description = "Check responsible AI",
            WebhookUrl = "https://aka.ms/lunaresponsibleai",
            IsEnabled = true
        });

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateObjectId(Name, nameof(Name));

            ValidationUtils.ValidateHttpsUrl(WebhookUrl, nameof(WebhookUrl));

            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));

        }

        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "WebhookUrl", Required = Required.Always)]
        public string WebhookUrl { get; set; }

        [JsonProperty(PropertyName = "IsEnabled", Required = Required.Always)]
        public bool IsEnabled { get; set; }

        [JsonProperty(PropertyName = "CreatedTime", Required = Required.Default)]
        public DateTime? CreatedTime { get; set; }

        [JsonProperty(PropertyName = "LastUpdatedTime", Required = Required.Default)]
        public DateTime? LastUpdatedTime { get; set; }
    }
}
