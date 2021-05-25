using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Luna.Common.Utils;
using Luna.Publish.PublicClient.Enums;
using Newtonsoft.Json;

namespace Luna.Publish.Public.Client.DataContract
{
    public class BaseAPIVersionProp : UpdatableProperties
    {
        public static string example = JsonConvert.SerializeObject(new BaseAPIVersionProp(RealtimeEndpointAPIVersionType.AzureML.ToString())
        {
            Description = "API version from Azure ML workspace",
            AdvancedSettings = null,
            Type = "AzureML"
        });

        public BaseAPIVersionProp(string type)
        {
            this.Type = type;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));
            ValidationUtils.ValidateStringValueLength(AdvancedSettings, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(AdvancedSettings));
            ValidationUtils.ValidateStringValueLength(Type, ValidationUtils.INTERNAL_OR_PREDEFINED_STRING_MAX_LENGTH, nameof(Type));
        }

        public override void Update(UpdatableProperties properties)
        {
            var value = (BaseAPIVersionProp)properties;
            this.Description = value.Description ?? this.Description;
            this.AdvancedSettings = value.AdvancedSettings ?? this.AdvancedSettings;
        }

        [JsonProperty(PropertyName = "Type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "AdvancedSettings", Required = Required.Default)]
        public string AdvancedSettings { get; set; }

    }
}
