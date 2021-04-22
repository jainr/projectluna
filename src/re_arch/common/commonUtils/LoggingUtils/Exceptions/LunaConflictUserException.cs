using Luna.Common.Utils.LoggingUtils.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.LoggingUtils.Exceptions
{
    public class LunaConflictUserException : LunaUserException
    {
        public LunaConflictUserException(string message) :
            base(message, UserErrorCode.Conflict, System.Net.HttpStatusCode.Conflict)
        {

        }
    }
}
