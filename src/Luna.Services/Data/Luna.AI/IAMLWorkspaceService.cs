using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;
using Luna.Data.DataContracts.Luna.AI;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the workspace resource.
    /// </summary>
    public interface IAMLWorkspaceService
    {
        /// <summary>
        /// Gets all workspaces.
        /// </summary>
        /// <returns>A list of workspaces.</returns>
        Task<List<AMLWorkspace>> GetAllAsync();

        /// <summary>
        /// Gets an workspace by name.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to get.</param>
        /// <param name="returnSecret">If return AAD secret</param>
        /// <returns>The workspace.</returns>
        Task<AMLWorkspace> GetAsync(string workspaceName, bool returnSecret = false);

        /// <summary>
        /// Get all models from a workspace
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns>All models registered in the workspace</returns>
        Task<List<MLModelArtifact>> GetAllModelsAsync(string workspaceName);

        /// <summary>
        /// Get all endpoints from a workspace
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns>All endpoints published in the workspace</returns>
        Task<List<MLEndpointArtifact>> GetAllEndpointsAsync(string workspaceName);

        /// <summary>
        /// Get all compute clusters from a workspace
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns>All compute clusters in the workspace</returns>
        Task<List<AMLComputeCluster>> GetAllComputeClustersAsync(string workspaceName);

        /// <summary>
        /// Creates an workspace.
        /// </summary>
        /// <param name="workspace">The workspace to create.</param>
        /// <returns>The created workspace.</returns>
        Task<AMLWorkspace> CreateAsync(AMLWorkspace workspace);

        /// <summary>
        /// Updates an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to update.</param>
        /// <param name="workspace">The updated workspace.</param>
        /// <returns>The updated workspace.</returns>
        Task<AMLWorkspace> UpdateAsync(string workspaceName, AMLWorkspace workspace);

        /// <summary>
        /// Deletes an workspace.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to delete.</param>
        /// <returns>The deleted workspace.</returns>
        Task<AMLWorkspace> DeleteAsync(string workspaceName);

        /// <summary>
        /// Checks if an workspace exists.
        /// </summary>
        /// <param name="workspaceName">The name of the workspace to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string workspaceName);
    }
}