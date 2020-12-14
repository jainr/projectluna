using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the workspace resource.
    /// </summary>
    public interface IAzureSynapseWorkspaceService
    {
        /// <summary>
        /// Gets all workspaces.
        /// </summary>
        /// <returns>A list of workspaces.</returns>
        Task<List<AzureSynapseWorkspace>> GetAllAsync();

        /// <summary>
        /// Gets an workspace by name.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to get.</param>
        /// <param name="returnSecret">If return AAD secret</param>
        /// <returns>The workspace.</returns>
        Task<AzureSynapseWorkspace> GetAsync(string workspaceName, bool returnSecret = false);

        /// <summary>
        /// Creates an workspace.
        /// </summary>
        /// <param name="workspace">The workspace to create.</param>
        /// <returns>The created workspace.</returns>
        Task<AzureSynapseWorkspace> CreateAsync(AzureSynapseWorkspace workspace);

        /// <summary>
        /// Updates an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to update.</param>
        /// <param name="workspace">The updated workspace.</param>
        /// <returns>The updated workspace.</returns>
        Task<AzureSynapseWorkspace> UpdateAsync(string workspaceName, AzureSynapseWorkspace workspace);

        /// <summary>
        /// Deletes an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to delete.</param>
        /// <returns>The deleted workspace.</returns>
        Task<AzureSynapseWorkspace> DeleteAsync(string workspaceName);

        /// <summary>
        /// Checks if an workspace exists.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string workspaceName);
    }
}