﻿using Luna.Common.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public class WebhookProvisioningStepProp : BaseProvisioningStepProp
    {
        public WebhookProvisioningStepProp()
        {
            this.IsSynchronized = false;
            this.TimeoutInSeconds = 30;
        }

        [OnDeserialized]
        internal new void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateHttpsUrl(WebhookUrl, nameof(WebhookUrl));
            ValidationUtils.ValidateEnum(this.WebhookAuthType, typeof(WebhookAuthType), nameof(WebhookAuthType));            base.OnDeserializedMethod(context);
        }

        [JsonProperty(PropertyName = "WebhookUrl", Required = Required.Always)]
        public string WebhookUrl { get; set; }

        [JsonProperty(PropertyName = "WebhookAuthType", Required = Required.Always)]
        public string WebhookAuthType { get; set; }

        [JsonProperty(PropertyName = "WebhookAuthKey", Required = Required.Default)]
        public string WebhookAuthKey { get; set; }

        [JsonProperty(PropertyName = "WebhookAuthValue", Required = Required.Default)]
        public string WebhookAuthValue { get; set; }

        [JsonProperty(PropertyName = "TimeoutInSeconds", Required = Required.Always)]
        public int TimeoutInSeconds { get; set; }

        [JsonProperty(PropertyName = "InputParameterNames", Required = Required.Always)]
        public List<string> InputParameterNames { get; set; }

    }
}