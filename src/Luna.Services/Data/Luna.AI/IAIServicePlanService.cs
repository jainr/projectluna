using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.DataContracts;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the AIServicePlan resource.
    /// </summary>
    public interface IAIServicePlanService
    {
        /// <summary>
        /// Gets all AIServicePlans.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <returns>A list of deployments.</returns>
        Task<List<AIServicePlan>> GetAllAsync(string aiServiceName);

        /// <summary>
        /// Gets an AIServicePlan by name.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>The AIServicePlan.</returns>
        Task<AIServicePlan> GetAsync(string aiServiceName, string aiServicePlanName);

        /// <summary>
        /// Creates an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlan">The AI service plan to create.</param>
        /// <returns>The created AI service plan.</returns>
        Task<AIServicePlan> CreateAsync(string aiServiceName, AIServicePlan aiServicePlan);

        /// <summary>
        /// Updates an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to update.</param>
        /// <param name="aiServicePlan">The AI service plan to update.</param>
        /// <returns>The updated AIServicePlan.</returns>
        Task<AIServicePlan> UpdateAsync(string aiServiceName, string aiServicePlanName, AIServicePlan aiServicePlan);

        /// <summary>
        /// Deletes an AIServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>The deleted AIServicePlan.</returns>
        Task<AIServicePlan> DeleteAsync(string aiServiceName, string aiServicePlanName);

        /// <summary>
        /// Checks if an AIServicePlan exists.
        /// </summary>
        /// <param name="aiServiceName">The name of the AI service.</param>
        /// <param name="aiServicePlanName">The name of the AI service plan to get.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsAsync(string aiServiceName, string aiServicePlanName);
    }
}