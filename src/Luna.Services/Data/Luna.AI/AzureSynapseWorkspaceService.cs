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

namespace Luna.Services.Data.Luna.AI
{
    public class AzureSynapseWorkspaceService : IAzureSynapseWorkspaceService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<AzureSynapseWorkspaceService> _logger;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly IOptionsMonitor<AzureConfigurationOption> _options;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="options">The Azure config info.</param>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="keyVaultHelper">The key vault helper.</param>
        public AzureSynapseWorkspaceService(IOptionsMonitor<AzureConfigurationOption> options,
            ISqlDbContext sqlDbContext, ILogger<AzureSynapseWorkspaceService> logger, IKeyVaultHelper keyVaultHelper)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyVaultHelper = keyVaultHelper;
        }

        /// <summary>
        /// Get all registered Azure Synapse workspaces
        /// </summary>
        /// <returns>The list of all registered Synapse workspaces</returns>
        public async Task<List<AzureSynapseWorkspace>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AzureSynapseWorkspace).Name));

            // Get all Azure Synapse workspace
            var workspaces = await _context.AzureSynapseWorkspaces.ToListAsync();

            // Do not return the secrets
            foreach (var workspace in workspaces)
            {
                workspace.AADApplicationSecrets = LunaConstants.SECRET_NOT_CHANGED_VALUE;
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AzureSynapseWorkspace).Name, workspaces.Count()));

            return workspaces;
        }

        /// <summary>
        /// Get an Azure Synapse workspace by name
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <param name="returnSecret">If return AAD secret</param>
        /// <returns></returns>
        public async Task<AzureSynapseWorkspace> GetAsync(string workspaceName, bool returnSecret = false)
        {
            if (!await ExistsAsync(workspaceName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(AzureSynapseWorkspace).Name,
                    workspaceName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(AzureSynapseWorkspace).Name, workspaceName));

            // Get the product that matches the provided productName
            var workspace = await _context.AzureSynapseWorkspaces.SingleOrDefaultAsync(o => (o.WorkspaceName == workspaceName));

            if (returnSecret)
            {
                workspace.AADApplicationSecrets = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, workspace.AADApplicationSecretName);
            }
            else
            {
                // Do not return the secrets
                workspace.AADApplicationSecrets = LunaConstants.SECRET_NOT_CHANGED_VALUE;
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(AzureSynapseWorkspace).Name,
               workspaceName,
               JsonSerializer.Serialize(workspace)));

            return workspace;
        }

        /// <summary>
        /// Check if the specified workspace exist
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string workspaceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(AzureSynapseWorkspace).Name, workspaceName));

            // Check that only one workspace with this workspaceName exists and has not been deleted
            var count = await _context.AzureSynapseWorkspaces
                .CountAsync(p => (p.WorkspaceName == workspaceName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(AzureSynapseWorkspace).Name, workspaceName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AzureSynapseWorkspace).Name, workspaceName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AzureSynapseWorkspace).Name, workspaceName, true));
                return true;
            }
        }

        /// <summary>
        /// Create a new workspace
        /// </summary>
        /// <param name="workspace">Azure Synapse workspace</param>
        /// <returns>Azure Synapse workspace</returns>
        public async Task<AzureSynapseWorkspace> CreateAsync(AzureSynapseWorkspace workspace)
        {
            if (workspace is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AzureSynapseWorkspace).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that a workspace with the same name does not already exist
            if (await ExistsAsync(workspace.WorkspaceName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(AzureSynapseWorkspace).Name,
                        workspace.WorkspaceName));
            }

            // Add secret to keyvault
            if (workspace.AADApplicationSecrets == null)
            {
                throw new LunaBadRequestUserException("AAD Application Secrets is needed with the Azure Synapse workspace", UserErrorCode.AuthKeyNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(AzureSynapseWorkspace).Name, workspace.WorkspaceName, payload: JsonSerializer.Serialize(workspace)));

            string secretName = string.Format(LunaConstants.SYNAPSE_SECRET_NAME_FORMAT, Context.GetRandomString(12));

            await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, workspace.AADApplicationSecrets));

            try
            {
                workspace.AADApplicationSecretName = secretName;
                _context.AzureSynapseWorkspaces.Add(workspace);
                await _context._SaveChangesAsync();
            }
            catch(Exception e)
            {
                // Try to delete the secret from key vault before failing the request
                await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, secretName));
                throw new LunaServerException(e.Message, innerException: e);
            }

            // Add workspace to db
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(AzureSynapseWorkspace).Name, workspace.WorkspaceName));

            return workspace;
        }

        /// <summary>
        /// Update an existing workspace
        /// </summary>
        /// <param name="workspace">Azure Synapse workspace</param>
        /// <returns>Azure Synapse workspace</returns>
        public async Task<AzureSynapseWorkspace> UpdateAsync(string workspaceName, AzureSynapseWorkspace workspace)
        {
            if (workspace is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AzureSynapseWorkspace).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (workspace.AADApplicationSecrets == null)
            {
                throw new LunaBadRequestUserException("AAD Application Secrets is needed with the Azure Synapse workspace", UserErrorCode.ArmTemplateNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(AzureSynapseWorkspace).Name, workspace.WorkspaceName, payload: JsonSerializer.Serialize(workspace)));

            var workspaceDb = await GetAsync(workspaceName);

            if (workspaceName != workspace.WorkspaceName)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AzureSynapseWorkspace).Name),
                    UserErrorCode.NameMismatch);
            }

            var oldSecretValue = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, workspaceDb.AADApplicationSecretName);

            if (workspace.AADApplicationSecrets.Equals(LunaConstants.SECRET_NOT_CHANGED_VALUE))
            {
                // Get secrets so we can connect to the workspace to get region info in the next step.
                workspace.AADApplicationSecrets = oldSecretValue;
            }

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
                _context.AzureSynapseWorkspaces.Update(workspaceDb);
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
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(AzureSynapseWorkspace).Name, workspace.WorkspaceName));

            return workspaceDb;
        }

        /// <summary>
        /// Delete a workspace
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns></returns>
        public async Task<AzureSynapseWorkspace> DeleteAsync(string workspaceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(AzureSynapseWorkspace).Name, workspaceName));

            var workspace = await GetAsync(workspaceName);

            // Remove the workspace from the db
            _context.AzureSynapseWorkspaces.Remove(workspace);
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
                    _logger.LogWarning($"Failed to delete secret {workspace.AADApplicationSecretName} while deleting Azure Synapse workspace {workspace.WorkspaceName}");
                }
            }

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(AzureSynapseWorkspace).Name, workspaceName));

            return workspace;
        }
    }
}
