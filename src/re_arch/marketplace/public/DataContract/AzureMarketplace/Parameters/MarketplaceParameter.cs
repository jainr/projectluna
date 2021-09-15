using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceParameter
    {
        public static string example = JsonConvert.SerializeObject(new MarketplaceParameter()
        {
            ParameterName = "region",
            DisplayName = "Region",
            Description = "The Azure region where resources are deployed to.",
            ValueType = MarketplaceParameterValueType.String.ToString(),
            FromList = true,
            ValueList = "westus;eastus"
        });

        public MarketplaceParameter()
        {
            FromList = false;
            IsRequired = false;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(ParameterName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(ParameterName));
            ValidationUtils.ValidateStringValueLength(DisplayName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(DisplayName));

            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));
            ValidationUtils.ValidateStringValueLength(ValueList, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(ValueList));
            ValidationUtils.ValidateEnum(ValueType, typeof(MarketplaceParameterValueType), nameof(ValueType));

        }

        [JsonProperty(PropertyName = "ParameterName", Required = Required.Always)]
        public string ParameterName { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "ValueType", Required = Required.Always)]
        public string ValueType { get; set; }

        [JsonProperty(PropertyName = "FromList", Required = Required.Default)]
        public bool FromList { get; set; }

        [JsonProperty(PropertyName = "ValueList", Required = Required.Default)]
        public string ValueList { get; set; }

        [JsonProperty(PropertyName = "Maximum", Required = Required.Default)]
        public int Maximum { get; set; }

        [JsonProperty(PropertyName = "Minimum", Required = Required.Default)]
        public int Minimum { get; set; }

        [JsonProperty(PropertyName = "IsRequired", Required = Required.Default)]
        public bool IsRequired { get; set; }

        [JsonProperty(PropertyName = "isUserInput", Required = Required.Default)]
        public bool IsUserInput { get; set; }

        [JsonProperty(PropertyName = "defaultValue", Required = Required.Default)]
        public object? DefaultValue { get; set; }
    }
}
