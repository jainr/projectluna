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

        /// <summary>
        /// Get ML compute service types
        /// </summary>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The compute service types</returns>
        Task<List<ServiceType>> GetMLComputeServiceTypes(LunaRequestHeaders headers);

        /// <summary>
        /// Get ML host service types
        /// </summary>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The host service types</returns>
        Task<List<ServiceType>> GetMLHostServiceTypes(LunaRequestHeaders headers);

        /// <summary>
        /// Get ML component types
        /// </summary>
        /// <param name="serviceType">The host service type</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The component types</returns>
        Task<List<ComponentType>> GetMLComponentTypes(string serviceType, LunaRequestHeaders headers);

        /// Get specified type of ML components from specified partner service
        /// </summary>
        /// <param name="serviceName">The partner service name</param>
        /// <param name="componentType">The component type</param>
        /// <param name="headers">The luna request headers</param>
        /// <returns>The ML components</returns>
        Task<List<BaseMLComponent>> GetMLComponents(string serviceName, string componentType, LunaRequestHeaders headers);

    }
}
