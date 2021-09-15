using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class ScriptProvisioningStepRequest : BaseProvisioningStepRequest
    {
        [JsonProperty(PropertyName = "scriptPackageUrl", Required = Required.Always)]
        public string ScriptPackageUrl { get; set; }

        [JsonProperty(PropertyName = "entryScriptFileName", Required = Required.Always)]
        public string EntryScriptFileName { get; set; }

        [JsonProperty(PropertyName = "timeoutInSeconds", Required = Required.Always)]
        public int TimeoutInSeconds { get; set; }

        [JsonProperty(PropertyName = "inputArguments", Required = Required.Always)]
        public List<ScriptArgumentRequest> InputArguments { get; set; }
    }
}
