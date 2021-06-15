using Luna.Common.Utils.RestClients;
using Luna.Partner.Public.Client;
using Luna.Publish.Public.Client.DataContract;
using Luna.Routing.Clients.MLServiceClients.Interfaces;
using Luna.Routing.Data.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Clients.MLServiceClients
{
    public class AzureSynapseClient : IPipelineEndpointClient
    {
        private AzureSynapseWorkspaceConfiguration _config;

        public AzureSynapseClient(AzureSynapseWorkspaceConfiguration config)
        {
            this._config = config;
        }

        /// <summary>
        /// Execute pipeline by calling the pipeline endpoint
        /// </summary>
        /// <param name="input">the input in JSON format</param>
        /// <param name="headers">The headers</param>
        /// <param name="predecessorOperationId">The predecessor operation id if specified</param>
        /// <returns>The operation id</returns>
        public async Task<string> ExecutePipeline(string input, LunaRequestHeaders headers, string predecessorOperationId = null)
        {
            return "";
        }

        /// <summary>
        /// Get the pipeline execution status
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="headers">The headers</param>
        /// <returns>The operation status</returns>
        public async Task<OperationStatus> GetPipelineExecutionStatus(string operationId, LunaRequestHeaders headers)
        {
            return null;
        }

        /// <summary>
        /// Get the pipeline execution output in Json format
        /// </summary>
        /// <param name="operationId">The operation id</param>
        /// <param name="headers">The headers</param>
        /// <returns>The execution output in Json format</returns>
        public async Task<string> GetPipelineExecutionJsonOutput(string operationId, LunaRequestHeaders headers)
        {
            return "";
        }

        Task<OperationStatus> IPipelineEndpointClient.ExecutePipeline(
            string appName, 
            string apiName, 
            string versionName, 
            string operationName, 
            string operationId,
            string input, BaseAPIVersionProp versionProperties, LunaRequestHeaders headers, string predecessorOperationId)
        {
            throw new NotImplementedException();
        }

        Task<List<OperationStatus>> IPipelineEndpointClient.ListOperations(BaseAPIVersionProp versionProperties, LunaRequestHeaders headers, string filterString = null)
        {
            throw new NotImplementedException();
        }

        Task IPipelineEndpointClient.CancelOperation(string operationId, BaseAPIVersionProp versionProperties, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        Task<OperationStatus> IPipelineEndpointClient.GetPipelineExecutionStatus(string operationId, BaseAPIVersionProp versionProperties, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }

        Task<object> IPipelineEndpointClient.GetPipelineExecutionJsonOutput(string operationId, BaseAPIVersionProp versionProperties, LunaRequestHeaders headers)
        {
            throw new NotImplementedException();
        }
    }
}
