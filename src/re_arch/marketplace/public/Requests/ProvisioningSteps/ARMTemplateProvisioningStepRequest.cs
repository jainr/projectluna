using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class ARMTemplateProvisioningStepRequest : BaseProvisioningStepRequest
    {
        [JsonProperty(PropertyName = "templateUrl", Required = Required.Always)]
        public string TemplateUrl { get; set; }

        [JsonProperty(PropertyName = "isRunInCompleteMode", Required = Required.Always)]
        public bool IsRunInCompleteMode { get; set; }

        [JsonProperty(PropertyName = "azureSubscriptionIdParameterName", Required = Required.Always)]
        public string AzureSubscriptionIdParameterName { get; set; }

        [JsonProperty(PropertyName = "azureLocationParameterName", Required = Required.Always)]
        public string AzureLocationParameterName { get; set; }

        [JsonProperty(PropertyName = "resourceGroupNameParameterName", Required = Required.Always)]
        public string ResourceGroupNameParameterName { get; set; }

        [JsonProperty(PropertyName = "accessTokenParameterName", Required = Required.Always)]
        public string AccessTokenParameterName { get; set; }

        [JsonProperty(PropertyName = "inputParameterNames", Required = Required.Always)]
        public List<string> InputParameterNames { get; set; }
    }
}
