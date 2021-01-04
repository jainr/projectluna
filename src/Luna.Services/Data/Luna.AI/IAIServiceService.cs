using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the AIService resource.
    /// </summary>
    public interface IAIServiceService
    {
        /// <summary>
        /// Gets all AIServices.
        /// </summary>
        /// <returns>A list of AIServices.</returns>
        Task<List<AIService>> GetAllAsync();

        /// <summary>
        /// Gets an AIService by name.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to get.</param>
        /// <returns>The AIService.</returns>
        Task<AIService> GetAsync(string aiServiceName);

        /// <summary>
        /// Gets a AIService by offer name
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <returns></returns>
        Task<AIService> GetByOfferIdAsync(long offerId);

        /// <summary>
        /// Creates an AIService.
        /// </summary>
        /// <param name="AIService">The AIService to create.</param>
        /// <returns>The created AIService.</returns>
        Task<AIService> CreateAsync(AIService AIService);

        /// <summary>
        /// Updates an AIService.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to update.</param>
        /// <param name="AIService">The updated AIService.</param>
        /// <returns>The updated AIService.</returns>
        Task<AIService> UpdateAsync(string aiServiceName, AIService AIService);

        /// <summary>
        /// Deletes an AIService.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to delete.</param>
        /// <returns>The deleted AIService.</returns>
        Task<AIService> DeleteAsync(string aiServiceName);

        /// <summary>
        /// Checks if an AIService exists.
        /// </summary>
        /// <param name="aiServiceName">The name of the AIService to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string aiServiceName);
    }
}