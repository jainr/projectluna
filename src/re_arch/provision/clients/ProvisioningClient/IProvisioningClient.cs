using Luna.Common.Utils;
using Luna.Provision.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients
{
    public interface IProvisioningClient
    {
        Task<bool> ResourceGroupExistsAsync(
            string subscriptionId,
            string resourceGroup,
            string accessToken,
            LunaRequestHeaders headers);

        Task<string> StartJumpBoxVMDeploymentAsync(
            string subscriptionId,
            string resourceGroup,
            string accessToken,
            LunaRequestHeaders headers);

        Task<DeploymentStatus> GetJumpBoxVMDeploymentStatusAsync(
            string subscriptionId,
            string resourceGroup,
            string accessToken,
            LunaRequestHeaders headers);

        Task<bool> StartApplicationDeploymentAsync(
            string host,
            string userName,
            string sshPrivateKey,
            string[] parameters,
            int timeoutInSeconds,
            LunaRequestHeaders headers);

        Task<DeploymentStatus> GetApplicationDeploymentStatusAsync(
            string host,
            string userName,
            string sshPrivateKey,
            LunaRequestHeaders headers);

        Task<DeploymentStatus> CleanupApplicationDeploymentStatusAsync(
            string host,
            string userName,
            string sshPrivateKey,
            LunaRequestHeaders headers);
    }
}
