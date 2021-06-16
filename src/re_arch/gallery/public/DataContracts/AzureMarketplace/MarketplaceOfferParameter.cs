using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class MarketplaceOfferParameter
    {
        public static string example = JsonConvert.SerializeObject(new MarketplaceOfferParameter()
        {
            ParameterName = "region",
            DisplayName = "Region",
            Description = "The Azure region where resources are deployed to.",
            ValueType = MarketplaceParameterValueType.STRING,
            FromList = true,
            ValueList = "westus;eastus"
        });

        [JsonProperty(PropertyName = "ParameterName", Required = Required.Always)]
        public string ParameterName { get; set; }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "ValueType", Required = Required.Always)]
        public string ValueType { get; set; }

        [JsonProperty(PropertyName = "FromList", Required = Required.Always)]
        public bool FromList { get; set; }

        [JsonProperty(PropertyName = "ValueList", Required = Required.Always)]
        public string ValueList { get; set; }

        [JsonProperty(PropertyName = "Maximum", Required = Required.Always)]
        public int Maximum { get; set; }

        [JsonProperty(PropertyName = "Minimum", Required = Required.Always)]
        public int Minimum { get; set; }
    }
}
