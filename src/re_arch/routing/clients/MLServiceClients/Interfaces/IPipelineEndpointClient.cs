using Luna.Common.Utils;
using Luna.Publish.Public.Client;
using Luna.Routing.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients
{
    public interface IPipelineEndpointClient
    {
        bool NeedRefresh { get; }

        /// <summary>
        /// Execute pipeline by calling the pipeline endpoint
        /// </summary>
        /// <param name="appName">the application name</param>
        /// <param name="apiName">the API name</param>
        /// <param name="versionName">the version name</param>
        /// <param name="operationName">the operation name</param>
        /// <param name="operationId">the operation id</param>
        /// <param name="input">the input in JSON format</param>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <param name="predecessorOperationId">The predecessor operation id if specified</param>
        /// <returns>The operation id</returns>
        Task<OperationStatus> ExecutePipeline(string appName,
            string apiName,
            string versionName,
            string operationName,
            string operationId,
            string input,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers,
            string predecessorOperationId = null);

        /// <summary>
        /// List operations
        /// </summary>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <param name="filterString">The filter string</param>
        /// <returns>The operations</returns>
        Task<List<OperationStatus>> ListOperations(BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers,
            string filterString = null);

        /// <summary>
        /// Cancel an operation
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <returns></returns>
        Task CancelOperation(string operationId,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers);

        /// <summary>
        /// Get the pipeline execution status
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <returns>The operation status</returns>
        Task<OperationStatus> GetPipelineExecutionStatus(string operationId,
            BaseAPIVersionProp versionProperties, 
            LunaRequestHeaders headers);

        /// <summary>
        /// Get the pipeline execution output in Json format
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="versionProperties">The version properties</param>
        /// <param name="headers">The headers</param>
        /// <returns>The execution output in Json format</returns>
        Task<object> GetPipelineExecutionJsonOutput(string operationId,
            BaseAPIVersionProp versionProperties,
            LunaRequestHeaders headers);

    }
}
