using Luna.Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Public.Client
{
    public class LunaAPIRequestJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.BaseType == typeof(BaseLunaAPIRequest);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jObject = JToken.ReadFrom(reader);

            if (jObject["type"] == null)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.MISSING_PARAMETER, "type"), 
                    UserErrorCode.InvalidInput);
            }

            object typeObj;

            if (!Enum.TryParse(typeof(LunaAPIType), jObject["type"].ToString(), out typeObj))
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.INVALID_LUNA_API_TYPE, jObject["type"].ToString()), 
                    UserErrorCode.InvalidInput);
            }

            BaseLunaAPIRequest result;
            switch ((LunaAPIType)typeObj)
            {
                case LunaAPIType.Realtime:
                    result = new RealtimeEndpointAPIRequest();
                    break;
                case LunaAPIType.Pipeline:
                    result = new PipelineEndpointAPIRequest();
                    break;
                case LunaAPIType.MLProject:
                    result = new MLProjectAPIRequest();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializer.Populate(jObject.CreateReader(), result);

            return result;
        }
    }
}
