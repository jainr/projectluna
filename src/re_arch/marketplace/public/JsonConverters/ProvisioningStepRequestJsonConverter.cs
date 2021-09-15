using Luna.Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Marketplace.Public.Client
{
    public class ProvisioningStepRequestJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.BaseType == typeof(BaseProvisioningStepRequest);
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

            if (!Enum.TryParse(typeof(MarketplaceProvisioningStepType), jObject["type"].ToString(), out typeObj))
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.INVALID_PROVISIONING_STEP_TYPE, jObject["type"].ToString()), 
                    UserErrorCode.InvalidInput);
            }

            BaseProvisioningStepRequest result;
            switch ((MarketplaceProvisioningStepType)typeObj)
            {
                case MarketplaceProvisioningStepType.Script:
                    result = new ScriptProvisioningStepRequest();
                    break;
                case MarketplaceProvisioningStepType.ARMTemplate:
                    result = new ARMTemplateProvisioningStepRequest();
                    break;
                case MarketplaceProvisioningStepType.Webhook:
                    result = new WebhookProvisioningStepRequest();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializer.Populate(jObject.CreateReader(), result);

            return result;
        }
    }
}
