using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.LoggingUtils.Exceptions
{
    public class LunaServerException : LunaException
    {
        public LunaServerException(
            string message,
            bool isRetryable = default,
            Exception innerException = default) : base(message)
        {
            this.IsRetryable = isRetryable;
        }

        public bool IsRetryable { get; set; }
    }
}
