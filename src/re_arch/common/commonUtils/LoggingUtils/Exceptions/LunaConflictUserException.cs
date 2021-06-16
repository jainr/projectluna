using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils
{
    public class LunaConflictUserException : LunaUserException
    {
        public LunaConflictUserException(string message) :
            base(message, UserErrorCode.Conflict, System.Net.HttpStatusCode.Conflict)
        {

        }
    }
}
