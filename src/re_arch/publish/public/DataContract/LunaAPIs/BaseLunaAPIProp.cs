using System.Runtime.Serialization;
using Luna.Common.Utils;
using Newtonsoft.Json;

namespace Luna.Publish.Public.Client
{
    public class BaseLunaAPIProp : UpdatableProperties
    {
        public static string example = JsonConvert.SerializeObject(new BaseLunaAPIProp(LunaAPIType.Realtime)
        {
            DisplayName = "sentimentanalysis",
            Description = "Sentiment analysis API",
            Type = "Pipeline",
            AdvancedSettings = null
        });

        public BaseLunaAPIProp()
        {

        }

        public BaseLunaAPIProp(LunaAPIType type)
        {
            this.Type = type.ToString();
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ValidationUtils.ValidateStringValueLength(DisplayName, ValidationUtils.OBJECT_NAME_STRING_MAX_LENGTH, nameof(DisplayName));
            ValidationUtils.ValidateStringValueLength(Description, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(Description));
            ValidationUtils.ValidateStringValueLength(AdvancedSettings, ValidationUtils.LONG_FREE_TEXT_STRING_MAX_LENGTH, nameof(AdvancedSettings));
            ValidationUtils.ValidateEnum(Type, typeof(LunaAPIType), nameof(Type));
        }

        public override void Update(UpdatableProperties properties)
        {
            var value = (BaseLunaAPIProp)properties;
            this.DisplayName = value.DisplayName ?? this.DisplayName;
            this.Description = value.Description ?? this.Description;
            this.AdvancedSettings = value.AdvancedSettings ?? this.AdvancedSettings;
        }

        [JsonProperty(PropertyName = "DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "Description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "AdvancedSettings", Required = Required.Default)]
        public string AdvancedSettings { get; set; }
    }
}
