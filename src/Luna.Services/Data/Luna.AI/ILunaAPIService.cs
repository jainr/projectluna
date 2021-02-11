using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the AIServicePlan resource.
    /// </summary>
    public interface ILunaAPIService
    {
        /// <summary>
        /// Gets all AIServicePlans.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <returns>A list of deployments.</returns>
        Task<List<LunaAPI>> GetAllAsync(string aiServiceName);

        /// <summary>
        /// Gets an AIServicePlan by name.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>The AIServicePlan.</returns>
        Task<LunaAPI> GetAsync(string aiServiceName, string aiServicePlanName);

        /// <summary>
        /// Creates an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlan">The AI service plan to create.</param>
        /// <returns>The created AI service plan.</returns>
        Task<LunaAPI> CreateAsync(string aiServiceName, LunaAPI aiServicePlan);

        /// <summary>
        /// Updates an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to update.</param>
        /// <param name="aiServicePlan">The AI service plan to update.</param>
        /// <returns>The updated AIServicePlan.</returns>
        Task<LunaAPI> UpdateAsync(string aiServiceName, string aiServicePlanName, LunaAPI aiServicePlan);

        /// <summary>
        /// Deletes an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>The deleted AIServicePlan.</returns>
        Task<LunaAPI> DeleteAsync(string aiServiceName, string aiServicePlanName);

        /// <summary>
        /// Checks if an AIServicePlan exists.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string aiServiceName, string aiServicePlanName);
    }
}