using Luna.Gallery.Public.Client;
using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients
{
    public class ScriptProvisionStep : BaseProvisionStep, IAsyncProvisionStep
    {
        private const string LOG_FILE_NAME = "log.txt";
        private const string ERROR_LOG_FILE_NAME = "error.txt";
        private const string WORKING_DIR_PARAM_NAME = "luna-scirpt-working-dir";
        private const string STATUS_FILE_NAME = "result.txt";
        private const string COMPLETED_STATUS_CONTENT = "completed";
        private const string FAILED_STATUS_CONTENT = "failed";

        public ScriptProvisioningStepProp Properties { get; set; }

        public async Task<ProvisionStepExecutionResult> CheckExecutionStatusAsync(List<MarketplaceSubscriptionParameter> parameters)
        {
            var remoteUtils = GetSshUtils(parameters);
            var content = remoteUtils.ReadFileContent(ERROR_LOG_FILE_NAME);
            if (content.Equals(COMPLETED_STATUS_CONTENT, StringComparison.InvariantCultureIgnoreCase))
            {
                return ProvisionStepExecutionResult.Completed;
            }
            else if (content.Equals(COMPLETED_STATUS_CONTENT, StringComparison.InvariantCultureIgnoreCase))
            {
                return ProvisionStepExecutionResult.Failed;
            }

            return ProvisionStepExecutionResult.Running;
        }

        public async Task<List<MarketplaceSubscriptionParameter>> FinishAsync(List<MarketplaceSubscriptionParameter> parameters)
        {
            var remoteUtils = GetSshUtils(parameters);
            remoteUtils.DeleteWorkingDirectory(GetParameterValue(parameters, WORKING_DIR_PARAM_NAME));
            return parameters;
        }

        public async Task<List<MarketplaceSubscriptionParameter>> StartAsync(List<MarketplaceSubscriptionParameter> parameters)
        {
            var remoteUtils = GetSshUtils(parameters);
            var workingDir = remoteUtils.ExecuteCommand(
                this.Properties.ScriptPackageUrl,
                this.Properties.EntryScriptFileName,
                parameters,
                LOG_FILE_NAME,
                ERROR_LOG_FILE_NAME);

            parameters.Add(new MarketplaceSubscriptionParameter()
            {
                Name = WORKING_DIR_PARAM_NAME,
                Type = MarketplaceParameterValueType.String.ToString(),
                Value = workingDir
            });

            return parameters;
        }

        private IRemoteUtils GetSshUtils(List<MarketplaceSubscriptionParameter> parameters)
        {
            var host = GetParameterValue(parameters, JumpboxParameterConstants.JUMPBOX_VM_PUBLIC_IP_PARAM_NAME);
            var userName = GetParameterValue(parameters, JumpboxParameterConstants.JUMPBOX_VM_USER_NAME_PARAM_NAME);
            var privateKey = GetParameterValue(parameters, JumpboxParameterConstants.JUMPBOX_VM_SSH_KEY_PARAM_NAME);
            var passPhrase = GetParameterValue(parameters, JumpboxParameterConstants.JUMPBOX_VM_SSH_PASS_PHRASE_PARAM_NAME);
            return new SshUtils(host, userName, privateKey, passPhrase);
        }

        private string GetParameterValue(List<MarketplaceSubscriptionParameter> parameters, string name)
        {
            var param = parameters.SingleOrDefault(x => x.Name == name);
            if (param != null)
            {
                return param.Value;
            }

            return string.Empty;
        }
    }
}
