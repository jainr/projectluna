using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils
{
    public abstract class LunaException: Exception
    {
        public LunaException(string message) : 
            base(message)
        {
        }

    }
}
