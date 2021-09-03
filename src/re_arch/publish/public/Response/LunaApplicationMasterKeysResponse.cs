using Newtonsoft.Json;

namespace Luna.Publish.Public.Client
{
    public class LunaApplicationMasterKeysResponse
    {
        public static string example = JsonConvert.SerializeObject(new LunaApplicationMasterKeysResponse()
        {
            PrimaryMasterKey = "92d3dd2a428540e7871aee92af26fc7d",
            SecondaryMasterKey = "60158cd36c464531bf40a6c7efc629cd",
        });

        [JsonProperty(PropertyName = "PrimaryMasterKey", Required = Required.Always)]
        public string PrimaryMasterKey { get; set; }

        [JsonProperty(PropertyName = "SecondaryMasterKey", Required = Required.Always)]
        public string SecondaryMasterKey { get; set; }
    }
}
