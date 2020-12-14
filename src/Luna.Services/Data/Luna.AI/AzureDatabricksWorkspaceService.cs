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
    public class AzureDatabricksWorkspaceService : IAzureDatabricksWorkspaceService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<AzureDatabricksWorkspaceService> _logger;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly IOptionsMonitor<AzureConfigurationOption> _options;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="options">The Azure config info.</param>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="keyVaultHelper">The key vault helper.</param>
        public AzureDatabricksWorkspaceService(IOptionsMonitor<AzureConfigurationOption> options,
            ISqlDbContext sqlDbContext, ILogger<AzureDatabricksWorkspaceService> logger, IKeyVaultHelper keyVaultHelper)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyVaultHelper = keyVaultHelper;
        }

        /// <summary>
        /// Get all registered Azure Databricks workspaces
        /// </summary>
        /// <returns>The list of all registered Azure Databricks workspaces</returns>
        public async Task<List<AzureDatabricksWorkspace>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(AzureDatabricksWorkspace).Name));

            // Get all products
            var workspaces = await _context.AzureDatabricksWorkspaces.ToListAsync();

            // Do not return the secrets
            foreach (var workspace in workspaces)
            {
                workspace.AADApplicationSecrets = LunaConstants.SECRET_NOT_CHANGED_VALUE;
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(AzureDatabricksWorkspace).Name, workspaces.Count()));

            return workspaces;
        }

        /// <summary>
        /// Get an Azure Databricks workspace by name
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <param name="returnSecret">If return AAD secret</param>
        /// <returns></returns>
        public async Task<AzureDatabricksWorkspace> GetAsync(string workspaceName, bool returnSecret = false)
        {
            if (!await ExistsAsync(workspaceName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(AzureDatabricksWorkspace).Name,
                    workspaceName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(AzureDatabricksWorkspace).Name, workspaceName));

            // Get the product that matches the provided productName
            var workspace = await _context.AzureDatabricksWorkspaces.SingleOrDefaultAsync(o => (o.WorkspaceName == workspaceName));

            if (returnSecret)
            {
                workspace.AADApplicationSecrets = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, workspace.AADApplicationSecretName);
            }
            else
            {
                // Do not return the secrets
                workspace.AADApplicationSecrets = LunaConstants.SECRET_NOT_CHANGED_VALUE;
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(AzureDatabricksWorkspace).Name,
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
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(AzureDatabricksWorkspace).Name, workspaceName));

            // Check that only one workspace with this workspaceName exists and has not been deleted
            var count = await _context.AzureDatabricksWorkspaces
                .CountAsync(p => (p.WorkspaceName == workspaceName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(AzureDatabricksWorkspace).Name, workspaceName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AzureDatabricksWorkspace).Name, workspaceName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(AzureDatabricksWorkspace).Name, workspaceName, true));
                return true;
            }
        }

        /// <summary>
        /// Create a new workspace
        /// </summary>
        /// <param name="workspace">Azure Databricks workspace</param>
        /// <returns>Azure Databricks workspace</returns>
        public async Task<AzureDatabricksWorkspace> CreateAsync(AzureDatabricksWorkspace workspace)
        {
            if (workspace is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AzureDatabricksWorkspace).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that a workspace with the same name does not already exist
            if (await ExistsAsync(workspace.WorkspaceName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(AzureDatabricksWorkspace).Name,
                        workspace.WorkspaceName));
            }

            // Add secret to keyvault
            if (workspace.AADApplicationSecrets == null)
            {
                throw new LunaBadRequestUserException("AAD Application Secrets is needed with the Azure Databricks workspace", UserErrorCode.AuthKeyNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(AzureDatabricksWorkspace).Name, workspace.WorkspaceName, payload: JsonSerializer.Serialize(workspace)));

            string secretName = string.Format(LunaConstants.ADB_SECRET_NAME_FORMAT, Context.GetRandomString(12));

            await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, workspace.AADApplicationSecrets));

            try
            {
                workspace.AADApplicationSecretName = secretName;
                _context.AzureDatabricksWorkspaces.Add(workspace);
                await _context._SaveChangesAsync();
            }
            catch(Exception e)
            {
                // Try to delete the secret from key vault before failing the request
                await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, secretName));
                throw new LunaServerException(e.Message, innerException: e);
            }

            // Add workspace to db
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(AzureDatabricksWorkspace).Name, workspace.WorkspaceName));

            return workspace;
        }

        /// <summary>
        /// Update an existing workspace
        /// </summary>
        /// <param name="workspace">Azure Databricks workspace</param>
        /// <returns>Azure Databricks workspace</returns>
        public async Task<AzureDatabricksWorkspace> UpdateAsync(string workspaceName, AzureDatabricksWorkspace workspace)
        {
            if (workspace is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(AzureDatabricksWorkspace).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (workspace.AADApplicationSecrets == null)
            {
                throw new LunaBadRequestUserException("AAD Application Secrets is needed with the Azure Databricks workspace", UserErrorCode.ArmTemplateNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(AzureDatabricksWorkspace).Name, workspace.WorkspaceName, payload: JsonSerializer.Serialize(workspace)));

            var workspaceDb = await GetAsync(workspaceName);

            if (workspaceName != workspace.WorkspaceName)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(AzureDatabricksWorkspace).Name),
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
                _context.AzureDatabricksWorkspaces.Update(workspaceDb);
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
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(AzureDatabricksWorkspace).Name, workspace.WorkspaceName));

            return workspaceDb;
        }

        /// <summary>
        /// Delete a workspace
        /// </summary>
        /// <param name="workspaceName">The workspace name</param>
        /// <returns></returns>
        public async Task<AzureDatabricksWorkspace> DeleteAsync(string workspaceName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(AzureDatabricksWorkspace).Name, workspaceName));

            var workspace = await GetAsync(workspaceName);

            // Remove the workspace from the db
            _context.AzureDatabricksWorkspaces.Remove(workspace);
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
                    _logger.LogWarning($"Failed to delete secret {workspace.AADApplicationSecretName} while deleting Azure Databricks workspace {workspace.WorkspaceName}");
                }
            }

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(AzureDatabricksWorkspace).Name, workspaceName));

            return workspace;
        }
    }
}
