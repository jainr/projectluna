using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils
{
    public class LunaNotFoundUserException : LunaUserException
    {
        public LunaNotFoundUserException(string message) :
            base(message, UserErrorCode.ResourceNotFound, System.Net.HttpStatusCode.NotFound)
        {

        }
    }
}
