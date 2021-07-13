using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class JumpboxParameterConstants
    {
        public const string JUMPBOX_VM_PUBLIC_IP_PARAM_NAME = "luna-jumpbox-vm-ip";
        public const string JUMPBOX_VM_USER_NAME_PARAM_NAME = "luna-jumpbox-user-name";
        public const string JUMPBOX_VM_SSH_PRIVATE_KEY_PARAM_NAME = "luna-jumpbox-ssh-private-key";
        public const string JUMPBOX_VM_SSH_PUBLIC_KEY_PARAM_NAME = "luna-jumpbox-ssh-public-key";
        public const string JUMPBOX_VM_SSH_PASS_PHRASE_PARAM_NAME = "luna-jumpbox-pass-phrase";

        public const string JUMPBOX_VM_NAME_PARAM_NAME = "luna-jumpbox-vm-name";
        public const string JUMPBOX_VM_LOCATION_PARAM_NAME = "luna-jumpbox-azure-location";
        public const string JUMPBOX_VM_SUB_ID_PARAM_NAME = "luna-jumpbox-azure-sub-id";
        public const string JUMPBOX_VM_RG_PARAM_NAME = "luna-jumpbox-azure-rg-name";
        public const string JUMPBOX_VM_ACCESS_TOKEN_PARAM_NAME = "luna-jumpbox-access-token";

        public static bool VerifyJumpboxParameterNames(List<string> parameterNames)
        {
            bool hasConnectionInfo = HasConnectionInfo(parameterNames);

            bool hasCreationInfo = HasCreationInfo(parameterNames);

            return hasConnectionInfo || hasCreationInfo;
        }

        public static bool HasConnectionInfo(List<string> parameterNames)
        {
            return parameterNames.Contains(JUMPBOX_VM_PUBLIC_IP_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_USER_NAME_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_SSH_PRIVATE_KEY_PARAM_NAME);
        }

        public static bool HasCreationInfo(List<string> parameterNames)
        {
            return parameterNames.Contains(JUMPBOX_VM_NAME_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_SUB_ID_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_RG_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_ACCESS_TOKEN_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_LOCATION_PARAM_NAME);
        }
    }
}
