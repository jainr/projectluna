using Luna.Common.Utils.RestClients;
using Luna.Partner.PublicClient.DataContract;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.PublicClient.Clients
{
    public interface IPartnerServiceClient
    {
        /// <summary>
        /// Get Azure ML workspace configuration
        /// </summary>
        /// <param name="name">The name of the workspace</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The workspace configuration</returns>
        Task<AzureMLWorkspaceConfiguration> GetAzureMLWorkspaceConfiguration(string name, LunaRequestHeaders headers);

        /// <summary>
        /// List all Azure ML workspaces
        /// </summary>
        /// <param name="headers">The luna request headers</param>
        /// <returns>All registered Azure ML workspaces</returns>
        Task<List<PartnerService>> ListAzureMLWorkspaces(LunaRequestHeaders headers);

        /// <summary>
        /// Register a new AML workspace as partner service
        /// </summary>
        /// <param name="name">The name of partner service</param>
        /// <param name="config">The configuration</param>
        /// <param name="headers">The request headers</param>
        /// <returns></returns>
        Task<AzureMLWorkspaceConfiguration> RegisterAzureMLWorkspace(
            string name,
            AzureMLWorkspaceConfiguration config,
            LunaRequestHeaders headers);

        /// <summary>
        /// Update an AML workspace as partner service
        /// </summary>
        /// <param name="name">The name of partner service</param>
        /// <param name="config">The configuration</param>
        /// <param name="headers">The request headers</param>
        /// <returns></returns>
        Task<AzureMLWorkspaceConfiguration> UpdateAzureMLWorkspace(
            string name,
            AzureMLWorkspaceConfiguration config,
            LunaRequestHeaders headers);

        /// <summary>
        /// Delete an AML workspace as partner service
        /// </summary>
        /// <param name="name">The name of partner service</param>
        /// <param name="headers">The request headers</param>
        /// <returns></returns>
        Task<bool> DeleteAzureMLWorkspace(
            string name,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get Azure Synapse workspace configuration
        /// </summary>
        /// <param name="name">The name of the workspace</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The workspace configuration</returns>
        Task<AzureSynapseWorkspaceConfiguration> GetAzureSynapseWorkspaceConfiguration(string name, LunaRequestHeaders headers);

    }
}
