using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class IaaSParameterConstants
    {
        public const string SUBSCRIPTION_ID_PARAM_NAME = "subscriptionId";
        public const string RESOURCE_GROUP_PARAM_NAME = "resourceGroup";
        public const string REGION_PARAM_NAME = "region";

        public static bool VerifyIaaSParameters(List<string> parameterNames)
        {
            return parameterNames.Contains(SUBSCRIPTION_ID_PARAM_NAME) &&
                parameterNames.Contains(RESOURCE_GROUP_PARAM_NAME) &&
                parameterNames.Contains(REGION_PARAM_NAME);
        }
    }
}
