using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.LoggingUtils
{
    public class ErrorMessages
    {
        public const string APPLICATION_ALREADY_EXIST = "Name of {0} is being used by other applications. Please choose a different name and try again.";
        public const string API_ALREADY_EXIST = "Name of {0} is being used by other APIs in current application {1}. Please choose a different name and try again.";
        public const string API_VERSION_ALREADY_EXIST = "Name of {0} is being used by other versions in current API {1}. Please choose a different name and try again.";
        public const string APPLICATION_DOES_NOT_EXIST = "Application {0} does not exist or you do not have permission to access it.";
        public const string API_DOES_NOT_EXIST = "API {0} in application {1} does not exist or you do not have permission to access it.";
        public const string API_VERSION_DOES_NOT_EXIST = "API version {0} in API {1} does not exist or you do not have permission to access it.";
        public const string MISSING_PARAMETER = "The parameter {0} is required.";
        public const string VALUE_NOT_UPDATABLE = "The value of parameter {0} is not updatable.";
        public const string CAN_NOT_DELETE_APPLICATION_WITH_APIS = "Can not delete the application {0} unless all APIs in the current application is deleted.";
        public const string API_TYPE_NOT_SUPPORTED = "API type {0} is not supported.";
        public const string VERSION_TYPE_NOT_SUPPORTED = "Version type {0} of API type {1} is not supported.";
    }
}
