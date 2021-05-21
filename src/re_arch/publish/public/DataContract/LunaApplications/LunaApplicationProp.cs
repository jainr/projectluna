using Luna.Common.LoggingUtils;
using Luna.Common.Utils;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Publish.Public.Client.DataContract
{
    public class LunaApplicationProp : UpdatableProperties
    {
        public static string example = JsonConvert.SerializeObject(new LunaApplicationProp()
        {
            OwnerUserId = Guid.NewGuid().ToString(),
            DisplayName = "My App",
            Description = "This is my application",
            DocumentationUrl = "https://aka.ms/lunadoc",
            LogoImageUrl = "https://aka.ms/lunalogo.png",
            Publisher = "Microsoft",
            Tags = new List<LunaApplicationTag>(
                new LunaApplicationTag[] 
                { 
                    new LunaApplicationTag() 
                    { 
                        Key = "Department", 
                        Value = "HR" 
                    } 
                })
        });

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(DisplayName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(DisplayName));
            ValidationUtils.ValidateStringValueLength(Publisher, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(Publisher));
            ValidationUtils.ValidateStringValueLength(OwnerUserId, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(OwnerUserId));
            ValidationUtils.ValidateStringValueLength(CreatedBy, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(CreatedBy));

            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));
            ValidationUtils.ValidateStringValueLength(DocumentationUrl, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(DocumentationUrl));
            ValidationUtils.ValidateStringValueLength(LogoImageUrl, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(LogoImageUrl));

            ValidationUtils.ValidateHttpsUrl(DocumentationUrl, nameof(DocumentationUrl));
            ValidationUtils.ValidateHttpsUrl(LogoImageUrl, nameof(LogoImageUrl));
        }

        public LunaApplicationProp()
        {
            Tags = new List<LunaApplicationTag>();
        }

        public override void Update(UpdatableProperties properties)
        {
            var value = (LunaApplicationProp)properties;
            this.DisplayName = value.DisplayName ?? this.DisplayName;
            this.Description = value.Description ?? this.Description;
            this.DocumentationUrl = value.DocumentationUrl ?? this.DocumentationUrl;
            this.LogoImageUrl = value.LogoImageUrl ?? this.LogoImageUrl;
            this.Publisher = value.Publisher ?? this.Publisher;
            this.Tags = (value.Tags == null || value.Tags.Count == 0) ? this.Tags : value.Tags;
        }

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
        public List<LunaApplicationTag> Tags { get; set; }

        [JsonProperty(PropertyName = "CreatedBy", Required = Required.Default)]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "PrimaryMasterKeySecretName", Required = Required.Default)]
        public string PrimaryMasterKeySecretName { get; set; }

        [JsonProperty(PropertyName = "SecondaryMasterKeySecretName", Required = Required.Default)]
        public string SecondaryMasterKeySecretName { get; set; }
    }
}
