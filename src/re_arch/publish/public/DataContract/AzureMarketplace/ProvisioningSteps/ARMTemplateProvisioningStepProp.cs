using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public class ARMTemplateProvisioningStepProp : BaseProvisioningStepProp
    {
        public ARMTemplateProvisioningStepProp()
        {
        }

        [OnDeserialized]
        internal new void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateHttpsUrl(TemplateUrl, nameof(TemplateUrl));
            base.OnDeserializedMethod(context);
        }

        [JsonProperty(PropertyName = "TemplateUrl", Required = Required.Always)]
        public string TemplateUrl { get; set; }

        [JsonProperty(PropertyName = "IsRunInCompleteMode", Required = Required.Always)]
        public bool IsRunInCompleteMode { get; set; }

        [JsonProperty(PropertyName = "AzureSubscriptionIdParameterName", Required = Required.Always)]
        public string AzureSubscriptionIdParameterName { get; set; }

        [JsonProperty(PropertyName = "AzureLocationParameterName", Required = Required.Always)]
        public string AzureLocationParameterName { get; set; }

        [JsonProperty(PropertyName = "ResourceGroupNameParameterName", Required = Required.Always)]
        public string ResourceGroupNameParameterName { get; set; }

        [JsonProperty(PropertyName = "AccessTokenParameterName", Required = Required.Always)]
        public string AccessTokenParameterName { get; set; }

        [JsonProperty(PropertyName = "InputParameterNames", Required = Required.Always)]
        public List<string> InputParameterNames { get; set; }

    }
}
