using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public class MarketplaceOfferProp
    {
        public MarketplaceOfferProp()
        {
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(DisplayName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(DisplayName));

            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));

        }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "IsManualActivation", Required = Required.Default)]
        public bool IsManualActivation { get; set; }
    }
}
