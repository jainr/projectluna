using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Gallery.Public.Client
{
    public class JumpboxParameterConstants
    {
        public const string JUMPBOX_VM_PUBLIC_IP_PARAM_NAME = "luna_jumpbox_vm_ip";
        public const string JUMPBOX_VM_USER_NAME_PARAM_NAME = "luna_jumpbox_user_name";
        public const string JUMPBOX_VM_SSH_KEY_PARAM_NAME = "luna_jumpbox_ssh_key";
        public const string JUMPBOX_VM_SSH_PASS_PHRASE_PARAM_NAME = "luna_jumpbox_pass_phrase";

        public const string JUMPBOX_VM_NAME_PARAM_NAME = "luna_jumpbox_vm_name";
        public const string JUMPBOX_VM_LOCATION_PARAM_NAME = "luna_jumpbox_azure_location";
        public const string JUMPBOX_VM_SUB_ID_PARAM_NAME = "luna_jumpbox_azure_sub_id";
        public const string JUMPBOX_VM_RG_PARAM_NAME = "luna_jumpbox_azure_rg_name";
        public const string JUMPBOX_VM_TENANT_ID_PARAM_NAME = "luna_jumpbox_azure_tenant_id";
        public const string JUMPBOX_VM_ACCESS_TOKEN_PARAM_NAME = "luna_jumpbox_access_token";

        public static bool VerifyJumpboxParameterNames(List<string> parameterNames)
        {
            bool hasConnectionInfo = parameterNames.Contains(JUMPBOX_VM_PUBLIC_IP_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_USER_NAME_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_SSH_KEY_PARAM_NAME);

            bool hasCreationInfo = parameterNames.Contains(JUMPBOX_VM_NAME_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_USER_NAME_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_SUB_ID_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_RG_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_TENANT_ID_PARAM_NAME) &&
                parameterNames.Contains(JUMPBOX_VM_ACCESS_TOKEN_PARAM_NAME);

            return hasConnectionInfo || hasCreationInfo;
        }
    }
}
