using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class WebhookProvisioningStepRequest : BaseProvisioningStepRequest
    {
        [JsonProperty(PropertyName = "webhookUrl", Required = Required.Always)]
        public string WebhookUrl { get; set; }

        [JsonProperty(PropertyName = "webhookAuthType", Required = Required.Always)]
        public string WebhookAuthType { get; set; }

        [JsonProperty(PropertyName = "webhookAuthKey", Required = Required.Default)]
        public string WebhookAuthKey { get; set; }

        [JsonProperty(PropertyName = "webhookAuthValue", Required = Required.Default)]
        public string WebhookAuthValue { get; set; }

        [JsonProperty(PropertyName = "timeoutInSeconds", Required = Required.Always)]
        public int TimeoutInSeconds { get; set; }

        [JsonProperty(PropertyName = "inputParameterNames", Required = Required.Always)]
        public List<string> InputParameterNames { get; set; }
    }
}
