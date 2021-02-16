using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Models.Provisioning;
using Luna.Data.DataContracts.Luna.AI;
using Luna.Data.Entities;

namespace Luna.Clients
{
    public interface IAMLClient
    {
        /// <summary>
        /// Get all models from an AML workspace
        /// </summary>
        /// <returns></returns>
        Task<List<MLModelArtifact>> GetModels(AMLWorkspace workspace);

        /// <summary>
        /// Get all endpoints from an AML workspace
        /// </summary>
        /// <returns></returns>
        Task<List<MLEndpointArtifact>> GetEndpoints(AMLWorkspace workspace);

        /// <summary>
        /// Get all compute cluster from an AML workspace
        /// </summary>
        /// <returns></returns>
        Task<List<AMLComputeCluster>> GetComputeClusters(AMLWorkspace workspace);
    }
}
