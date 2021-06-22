using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public class MarketplaceProvisioningStep
    {
        public MarketplaceProvisioningStep()
        {
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(Name, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(Name));

            ValidationUtils.ValidateEnum(Type, typeof(MarketplaceProvisioningStepType), nameof(Type));

        }

        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "Properties", Required = Required.Always)]
        public BaseProvisioningStepProp Properties { get; set; }
    }
}
