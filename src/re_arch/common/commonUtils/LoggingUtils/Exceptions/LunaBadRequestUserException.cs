﻿using Luna.Common.Utils.LoggingUtils.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.LoggingUtils.Exceptions
{
    public class LunaBadRequestUserException : LunaUserException
    {
        public LunaBadRequestUserException(string message, UserErrorCode code, string target = "method_error") : 
            base(message, code, System.Net.HttpStatusCode.BadRequest, target)
        {

        }
    }
}
