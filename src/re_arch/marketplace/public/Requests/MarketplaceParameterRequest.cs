using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class MarketplaceParameterRequest
    {
        [JsonProperty(PropertyName = "parameterName", Required = Required.Always)]
        public string ParameterName { get; set; }

        [JsonProperty(PropertyName = "displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "valueType", Required = Required.Always)]
        public string ValueType { get; set; }

        [JsonProperty(PropertyName = "fromList", Required = Required.Default)]
        public bool FromList { get; set; }

        [JsonProperty(PropertyName = "valueList", Required = Required.Default)]
        public string ValueList { get; set; }

        [JsonProperty(PropertyName = "maximum", Required = Required.Default)]
        public int Maximum { get; set; }

        [JsonProperty(PropertyName = "minimum", Required = Required.Default)]
        public int Minimum { get; set; }

        [JsonProperty(PropertyName = "isRequired", Required = Required.Default)]
        public bool IsRequired { get; set; }

        [JsonProperty(PropertyName = "isUserInput", Required = Required.Default)]
        public bool IsUserInput { get; set; }

        [JsonProperty(PropertyName = "defaultValue", Required = Required.Default)]
        public object? DefaultValue { get; set; }
    }
}
