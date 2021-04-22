using Luna.Common.Utils.LoggingUtils.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.LoggingUtils.Exceptions
{
    public class LunaNotSupportedUserException : LunaUserException
    {
        public LunaNotSupportedUserException(string message) :
            base(message, UserErrorCode.NotSupported, System.Net.HttpStatusCode.NotImplemented)
        {

        }
    }
}
