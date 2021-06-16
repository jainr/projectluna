using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils
{
    public class LunaNotSupportedUserException : LunaUserException
    {
        public LunaNotSupportedUserException(string message) :
            base(message, UserErrorCode.NotSupported, System.Net.HttpStatusCode.NotImplemented)
        {

        }
    }
}
