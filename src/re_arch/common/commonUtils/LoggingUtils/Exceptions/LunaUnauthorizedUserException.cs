using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils
{
    public class LunaUnauthorizedUserException : LunaUserException
    {
        public LunaUnauthorizedUserException(string message) :
            base(message, UserErrorCode.Unauthorized, System.Net.HttpStatusCode.Unauthorized)
        {

        }
    }
}
