using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class BaseProvisioningStepProp
    {
        public BaseProvisioningStepProp()
        {
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));

        }

        [JsonProperty(PropertyName = "IsSynchronized", Required = Required.Always)]
        public bool IsSynchronized { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

    }
}
