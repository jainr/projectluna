using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils
{
    /// <summary>
    /// The user error code for troublethooting
    /// You should NEVER delete any error code from this Enum
    /// </summary>
    public enum UserErrorCode
    {
        ResourceNotFound,
        PayloadNotProvided,
        NameMismatch,
        Conflict,
        ParameterNameIsReserved,
        ParameterNotProvided,
        ArmTemplateNotProvided,
        InvalidParameter,
        Unauthorized,
        AuthKeyNotProvided,
        InvalidToken,
        Disconnected,
        InternalServerError,
        MissingQueryParameter,
        NotSupported,
        CanNotPerformOperation,
        InvalidInput
    }
}
