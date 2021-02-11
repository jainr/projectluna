using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the AIService resource.
    /// </summary>
    public interface ILunaApplicationService
    {
        /// <summary>
        /// Gets all AIServices.
        /// </summary>
        /// <returns>A list of AIServices.</returns>
        Task<List<LunaApplication>> GetAllAsync();

        /// <summary>
        /// Gets an AIService by name.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to get.</param>
        /// <returns>The AIService.</returns>
        Task<LunaApplication> GetAsync(string aiServiceName);

        /// <summary>
        /// Creates an AIService.
        /// </summary>
        /// <param name="AIService">The AIService to create.</param>
        /// <returns>The created AIService.</returns>
        Task<LunaApplication> CreateAsync(LunaApplication AIService);

        /// <summary>
        /// Updates an AIService.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to update.</param>
        /// <param name="AIService">The updated AIService.</param>
        /// <returns>The updated AIService.</returns>
        Task<LunaApplication> UpdateAsync(string aiServiceName, LunaApplication AIService);

        /// <summary>
        /// Deletes an AIService.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to delete.</param>
        /// <returns>The deleted AIService.</returns>
        Task<LunaApplication> DeleteAsync(string aiServiceName);

        /// <summary>
        /// Checks if an AIService exists.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string aiServiceName);
    }
}