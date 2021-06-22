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

    }
}
