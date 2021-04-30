using Luna.Common.Utils.RestClients;
using Luna.Routing.Data.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients.MLServiceClients.Interfaces
{
    public interface IPipelineEndpointClient
    {
        /// <summary>
        /// Execute pipeline by calling the pipeline endpoint
        /// </summary>
        /// <param name="input">the input in JSON format</param>
        /// <param name="headers">The headers</param>
        /// <param name="predecessorOperationId">The predecessor operation id if specified</param>
        /// <returns>The operation id</returns>
        Task<string> ExecutePipeline(string input, LunaRequestHeaders headers, string predecessorOperationId = null);

        /// <summary>
        /// Get the pipeline execution status
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="headers">The headers</param>
        /// <returns>The operation status</returns>
        Task<OperationStatus> GetPipelineExecutionStatus(string operationId, LunaRequestHeaders headers);

        /// <summary>
        /// Get the pipeline execution output in Json format
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="headers">The headers</param>
        /// <returns>The execution output in Json format</returns>
        Task<string> GetPipelineExecutionJsonOutput(string operationId, LunaRequestHeaders headers);

    }
}
