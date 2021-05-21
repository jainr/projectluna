using Luna.Common.LoggingUtils;
using Luna.Common.Utils.LoggingUtils.Enums;
using Luna.Common.Utils.LoggingUtils.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Common.Utils
{
    public class ValidationUtils
    {
        /// <summary>
        /// Max length of internal or pre-defined (types, enums) strings
        /// </summary>
        public static int INTERNAL_OR_PREDEFINED_STRING_MAX_LENGTH = 64;

        /// <summary>
        /// Max length of object names (application name, user id...)
        /// </summary>
        public static int OBJECT_NAME_STRING_MAX_LENGTH = 128;

        /// <summary>
        /// Max length of free text string (description, urls...)
        /// </summary>
        public static int LONG_FREE_TEXT_STRING_MAX_LENGTH = 1024;

        public static void ValidateInput<T>(string content)
        {
            var input = JsonConvert.DeserializeObject<T>(content);

            if (input == null)
            {
                throw new LunaBadRequestUserException(ErrorMessages.INVALID_INPUT, UserErrorCode.InvalidInput);
            }
        }

        public static void ValidateEnum(string value, Type type, string propertyName = "")
        {
            object result;
            if (!Enum.TryParse(type, value, out result))
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.INVALID_ENUM_PROPERTY_VALUE, propertyName),
                    UserErrorCode.InvalidParameter,
                    target: propertyName);
            }
        }

        public static void ValidateStringValueLength(string value, int maxLength, string propertyName = "")
        {
            if (value !=null && value.Length > maxLength)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.STRING_PROPERTY_VALUE_TOO_LONG, propertyName, maxLength),
                    UserErrorCode.InvalidParameter,
                    target: propertyName);
            }
        }

        public static void ValidateObjectId(string value, string propertyName)
        {
            // Version names are all lower case or number and max 128 chars
            if (value != null && !value.All(x => char.IsLower(x) || char.IsNumber(x)))
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.INVALID_ID_PROPERTY, propertyName),
                    UserErrorCode.InvalidParameter,
                    target: propertyName);
            }

            if (value != null && value.Length > OBJECT_NAME_STRING_MAX_LENGTH)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.STRING_PROPERTY_VALUE_TOO_LONG, propertyName, OBJECT_NAME_STRING_MAX_LENGTH),
                    UserErrorCode.InvalidParameter,
                    target: propertyName);
            }
        }

        public static void ValidateHttpsUrl(string value, string propertyName = "")
        {
            Uri uriResult;

            if (value != null && (!Uri.TryCreate(value, UriKind.Absolute, out uriResult)
                || uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.STRING_PROPERTY_NOT_VALID_HTTPS_URL, propertyName),
                    UserErrorCode.InvalidParameter,
                    target: propertyName);
            }
        }
    }
}
