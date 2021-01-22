using Luna.Clients.Azure.Auth;
using Luna.Clients.Controller;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Luna.Services.Utilities.ExpressionEvaluation;
using Luna.Clients.Azure;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Luna.Data.Constants;
using Luna.Clients;
using Luna.Data.DataContracts.Luna.AI;

namespace Luna.Services.Data.Luna.AI
{
    public class AMLWorkspaceService : IAMLWorkspaceService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<AMLWorkspaceService> _logger;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly IAMLClient _amlClient;
        private readonly IOptionsMonitor<AzureConfigurationOption> _options;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="options">The Azure config info.</param>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="keyVaultHelper">The key vault helper.</param>
        public AMLWorkspaceService(IOptionsMonitor<AzureConfigurationOption> options, IAMLClient amlClient,
            ISqlDbContext sqlDbContext, ILogger<AMLWorkspaceService> logger, IKeyVaultHelper keyVaultHelper)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _amlClient = amlClient ?? throw new ArgumentNullException(nameof(options));
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyVaultHelper = keyVaultHelper;
        }

        /// <summary>
        /// Get all registered AML workspaces
        /// </summary>
        /// <returns>The list of all registere AML workspaces</returns>
        public async Task<List<AMLWorkspace>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AMLWorkspace).Name));

            // Get all products
            var workspaces = await _context.AMLWorkspaces.ToListAsync();

            // Do not return the secrets
            foreach (var workspace in workspaces)
            {
                workspace.AADApplicationSecrets = LunaConstants.SECRET_NOT_CHANGED_VALUE;
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AMLWorkspace).Name, workspaces.Count()));

            return workspaces;
        }

        /// <summary>
        /// Get an AML workspace by name
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <param name="returnSecret">If return AAD secret</param>
        /// <returns></returns>
        public async Task<AMLWorkspace> GetAsync(string workspaceName, bool returnSecret = false)
        {
            if (!await ExistsAsync(workspaceName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(AMLWorkspace).Name,
                    workspaceName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(AMLWorkspace).Name, workspaceName));

            // Get the product that matches the provided productName
            var workspace = await _context.AMLWorkspaces.SingleOrDefaultAsync(o => (o.WorkspaceName == workspaceName));

            if (returnSecret)
            {
                workspace.AADApplicationSecrets = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, workspace.AADApplicationSecretName);
            }
            else
            {
                // Do not return the secrets
                workspace.AADApplicationSecrets = LunaConstants.SECRET_NOT_CHANGED_VALUE;
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(AMLWorkspace).Name,
               workspaceName,
               JsonSerializer.Serialize(workspace)));

            return workspace;
        }

        /// <summary>
        /// Get all models from a workspace
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns>All models registered in the workspace</returns>
        public async Task<List<MLModelArtifact>> GetAllModelsAsync(string workspaceName)
        {
            var workspace = await GetAsync(workspaceName, returnSecret: true);
            var models = await _amlClient.GetModels(workspace);
            return models;
        }

        /// <summary>
        /// Get all endpoints from a workspace
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns>All endpoints published in the workspace</returns>
        public async Task<List<MLEndpointArtifact>> GetAllEndpointsAsync(string workspaceName)
        {
            var workspace = await GetAsync(workspaceName, returnSecret: true);
            var endpoints = await _amlClient.GetEndpoints(workspace);
            return endpoints;
        }

        /// <summary>
        /// Get all compute clusters from a workspace
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns>All compute clusters in the workspace</returns>
        public async Task<List<AMLComputeCluster>> GetAllComputeClustersAsync(string workspaceName)
        {
            var workspace = await GetAsync(workspaceName, returnSecret: true);
            var clusters = await _amlClient.GetComputeClusters(workspace);
            return clusters;
        }

        /// <summary>
        /// Check if the specified workspace exist
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string workspaceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(AMLWorkspace).Name, workspaceName));

            // Check that only one workspace with this workspaceName exists and has not been deleted
            var count = await _context.AMLWorkspaces
                .CountAsync(p => (p.WorkspaceName == workspaceName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(AMLWorkspace).Name, workspaceName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AMLWorkspace).Name, workspaceName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AMLWorkspace).Name, workspaceName, true));
                return true;
            }
        }

        /// <summary>
        /// Create a new workspace
        /// </summary>
        /// <param name="workspace">AML workspace</param>
        /// <returns>AML workspace</returns>
        public async Task<AMLWorkspace> CreateAsync(AMLWorkspace workspace)
        {
            if (workspace is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AMLWorkspace).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that a workspace with the same name does not already exist
            if (await ExistsAsync(workspace.WorkspaceName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(AMLWorkspace).Name,
                        workspace.WorkspaceName));
            }

            // Add secret to keyvault
            if (workspace.AADApplicationSecrets == null)
            {
                throw new LunaBadRequestUserException("AAD Application Secrets is needed with the aml workspace", UserErrorCode.AuthKeyNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(AMLWorkspace).Name, workspace.WorkspaceName, payload: JsonSerializer.Serialize(workspace)));

            workspace.Region = await ControllerHelper.GetRegion(workspace);

            string secretName = string.Format(LunaConstants.AML_SECRET_NAME_FORMAT, Context.GetRandomString(12));

            await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, workspace.AADApplicationSecrets));

            try
            {
                workspace.AADApplicationSecretName = secretName;
                _context.AMLWorkspaces.Add(workspace);
                await _context._SaveChangesAsync();
            }
            catch(Exception e)
            {
                // Try to delete the secret from key vault before failing the request
                await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, secretName));
                throw new LunaServerException(e.Message, innerException: e);
            }

            // Add workspace to db
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(AMLWorkspace).Name, workspace.WorkspaceName));

            return workspace;
        }

        /// <summary>
        /// Update an existing workspace
        /// </summary>
        /// <param name="workspace">AML workspace</param>
        /// <returns>AML workspace</returns>
        public async Task<AMLWorkspace> UpdateAsync(string workspaceName, AMLWorkspace workspace)
        {
            if (workspace is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AMLWorkspace).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (workspace.AADApplicationSecrets == null)
            {
                throw new LunaBadRequestUserException("AAD Application Secrets is needed with the AML workspace", UserErrorCode.ArmTemplateNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(AMLWorkspace).Name, workspace.WorkspaceName, payload: JsonSerializer.Serialize(workspace)));

            var workspaceDb = await GetAsync(workspaceName);

            if (workspaceName != workspace.WorkspaceName)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AMLWorkspace).Name),
                    UserErrorCode.NameMismatch);
            }

            var oldSecretValue = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, workspaceDb.AADApplicationSecretName);

            if (workspace.AADApplicationSecrets.Equals(LunaConstants.SECRET_NOT_CHANGED_VALUE))
            {
                // Get secrets so we can connect to the workspace to get region info in the next step.
                workspace.AADApplicationSecrets = oldSecretValue;
            }

            workspace.Region = await ControllerHelper.GetRegion(workspace);

            // Copy over the changes
            workspaceDb.Copy(workspace);

            // update the secrets if needed
            if (!workspace.AADApplicationSecrets.Equals(oldSecretValue))
            {
                await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, workspaceDb.AADApplicationSecretName, workspace.AADApplicationSecrets));
            }

            try
            {
                // Update workspaceDb values and save changes in db
                _context.AMLWorkspaces.Update(workspaceDb);
                await _context._SaveChangesAsync();
            }
            catch(Exception e)
            {
                if (!workspace.AADApplicationSecrets.Equals(oldSecretValue))
                {
                    await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, workspaceDb.AADApplicationSecretName, oldSecretValue));
                }
                throw new LunaServerException(e.Message, innerException: e);
            }
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(AMLWorkspace).Name, workspace.WorkspaceName));

            return workspaceDb;
        }

        /// <summary>
        /// Delete a workspace
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns></returns>
        public async Task<AMLWorkspace> DeleteAsync(string workspaceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(AMLWorkspace).Name, workspaceName));

            var workspace = await GetAsync(workspaceName);

            // Remove the workspace from the db
            _context.AMLWorkspaces.Remove(workspace);
            await _context._SaveChangesAsync();

            // Delete secret from key vault
            if (!string.IsNullOrEmpty(workspace.AADApplicationSecretName))
            {
                try
                {
                    await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, workspace.AADApplicationSecretName));
                }
                catch 
                {
                    // Log the warning and ignore the exception
                    _logger.LogWarning($"Failed to delete secret {workspace.AADApplicationSecretName} while deleting AML workspace {workspace.WorkspaceName}");
                }
            }

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(AMLWorkspace).Name, workspaceName));

            return workspace;
        }
    }
}
