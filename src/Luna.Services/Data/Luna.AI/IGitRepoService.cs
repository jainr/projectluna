using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the workspace resource.
    /// </summary>
    public interface IGitRepoService
    {
        /// <summary>
        /// Gets all git repo.
        /// </summary>
        /// <returns>A list of workspaces.</returns>
        Task<List<GitRepo>> GetAllAsync();

        /// <summary>
        /// Gets a git repo by name.
        /// </summary>
        /// <param name="repoName">The name of the git repo to get.</param>
        /// <param name="returnSecret">If return secret</param>
        /// <returns>The workspace.</returns>
        Task<GitRepo> GetAsync(string repoName, bool returnSecret = false);

        /// <summary>
        /// Creates a Git repo.
        /// </summary>
        /// <param name="gitRepo">The git repo to create.</param>
        /// <returns>The created git repo.</returns>
        Task<GitRepo> CreateAsync(GitRepo gitRepo);

        /// <summary>
        /// Updates a Git repo.
        /// </summary>
        /// <param name="repoName">The name of the Git repo to update.</param>
        /// <param name="gitRepo">The updated workspace.</param>
        /// <returns>The updated workspace.</returns>
        Task<GitRepo> UpdateAsync(string repoName, GitRepo gitRepo);

        /// <summary>
        /// Deletes a Git repo.
        /// </summary>
        /// <param name="repoName">The name of the Git repo to delete.</param>
        /// <returns>The deleted workspace.</returns>
        Task<GitRepo> DeleteAsync(string workspaceName);

        /// <summary>
        /// Checks if a Git repo exists.
        /// </summary>
        /// <param name="repoName">The name of the Git repo to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string repoName);
    }
}