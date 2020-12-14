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
    public class GitRepoService : IGitRepoService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<GitRepoService> _logger;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly IOptionsMonitor<AzureConfigurationOption> _options;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="options">The Azure config info.</param>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="keyVaultHelper">The key vault helper.</param>
        public GitRepoService(IOptionsMonitor<AzureConfigurationOption> options,
            ISqlDbContext sqlDbContext, ILogger<GitRepoService> logger, IKeyVaultHelper keyVaultHelper)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyVaultHelper = keyVaultHelper;
        }

        /// <summary>
        /// Get all registered Git repo
        /// </summary>
        /// <returns>The list of all registered Git repo</returns>
        public async Task<List<GitRepo>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(GitRepo).Name));

            // Get all Git repos
            var gitRepos = await _context.GitRepos.ToListAsync();

            // Do not return the secrets
            foreach (var gitRepo in gitRepos)
            {
                gitRepo.PersonalAccessToken = LunaConstants.SECRET_NOT_CHANGED_VALUE;
            }
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(GitRepo).Name, gitRepos.Count()));

            return gitRepos;
        }

        /// <summary>
        /// Get a Git repo by name
        /// </summary>
        /// <param name="repoName">The repo name</param>
        /// <param name="returnSecret">If return secret</param>
        /// <returns></returns>
        public async Task<GitRepo> GetAsync(string repoName, bool returnSecret = false)
        {
            if (!await ExistsAsync(repoName))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(GitRepo).Name,
                    repoName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(GitRepo).Name, repoName));

            // Get the git repo that matches the provided repo name
            var gitRepo = await _context.GitRepos.SingleOrDefaultAsync(o => (o.RepoName == repoName));

            if (returnSecret)
            {
                gitRepo.PersonalAccessToken = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, gitRepo.PersonalAccessTokenSecretName);
            }
            else
            {
                // Do not return the secrets
                gitRepo.PersonalAccessToken = LunaConstants.SECRET_NOT_CHANGED_VALUE;
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(GitRepo).Name,
               repoName,
               JsonSerializer.Serialize(gitRepo)));

            return gitRepo;
        }

        /// <summary>
        /// Check if the specified Git repo exist
        /// </summary>
        /// <param name="repoName">The Git repo name</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string repoName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(GitRepo).Name, repoName));

            // Check that only one gitRepo with this repoName exists and has not been deleted
            var count = await _context.GitRepos
                .CountAsync(p => (p.RepoName == repoName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(GitRepo).Name, repoName));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(GitRepo).Name, repoName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(GitRepo).Name, repoName, true));
                return true;
            }
        }

        /// <summary>
        /// Register a new Git repo
        /// </summary>
        /// <param name="gitRepo">Git repo</param>
        /// <returns>Git repo</returns>
        public async Task<GitRepo> CreateAsync(GitRepo gitRepo)
        {
            if (gitRepo is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(GitRepo).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that a gitRepo with the same name does not already exist
            if (await ExistsAsync(gitRepo.RepoName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(GitRepo).Name,
                        gitRepo.RepoName));
            }

            // Add secret to keyvault
            if (gitRepo.PersonalAccessTokenSecretName == null)
            {
                throw new LunaBadRequestUserException("Personal access token is needed with the Git Repo", UserErrorCode.AuthKeyNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(GitRepo).Name, gitRepo.RepoName, payload: JsonSerializer.Serialize(gitRepo)));

            string secretName = string.Format(LunaConstants.GIT_SECRET_NAME_FORMAT, Context.GetRandomString(12));

            await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, secretName, gitRepo.PersonalAccessToken));

            try
            {
                gitRepo.PersonalAccessToken = secretName;
                _context.GitRepos.Add(gitRepo);
                await _context._SaveChangesAsync();
            }
            catch(Exception e)
            {
                // Try to delete the secret from key vault before failing the request
                await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, secretName));
                throw new LunaServerException(e.Message, innerException: e);
            }

            // Add gitRepo to db
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(GitRepo).Name, gitRepo.RepoName));

            return gitRepo;
        }

        /// <summary>
        /// Update an existing gitRepo
        /// </summary>
        /// <param name="gitRepo">Git Repo</param>
        /// <returns>Git Repo</returns>
        public async Task<GitRepo> UpdateAsync(string repoName, GitRepo gitRepo)
        {
            if (gitRepo is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(GitRepo).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (gitRepo.PersonalAccessTokenSecretName == null)
            {
                throw new LunaBadRequestUserException("AAD Application Secrets is needed with the Git repo", UserErrorCode.ArmTemplateNotProvided);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(GitRepo).Name, gitRepo.RepoName, payload: JsonSerializer.Serialize(gitRepo)));

            var gitRepoDb = await GetAsync(repoName);

            if (repoName != gitRepo.RepoName)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(GitRepo).Name),
                    UserErrorCode.NameMismatch);
            }

            var oldSecretValue = await _keyVaultHelper.GetSecretAsync(_options.CurrentValue.Config.VaultName, gitRepoDb.PersonalAccessTokenSecretName);

            if (gitRepo.PersonalAccessToken.Equals(LunaConstants.SECRET_NOT_CHANGED_VALUE))
            {
                // Get secrets so we can connect to the gitRepo to get region info in the next step.
                gitRepo.PersonalAccessToken = oldSecretValue;
            }
            // Copy over the changes
            gitRepoDb.Copy(gitRepo);

            // update the secrets if needed
            if (!gitRepo.PersonalAccessToken.Equals(oldSecretValue))
            {
                await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, gitRepoDb.PersonalAccessTokenSecretName, gitRepo.PersonalAccessToken));
            }

            try
            {
                // Update gitRepoDb values and save changes in db
                _context.GitRepos.Update(gitRepoDb);
                await _context._SaveChangesAsync();
            }
            catch(Exception e)
            {
                if (!gitRepo.PersonalAccessToken.Equals(oldSecretValue))
                {
                    await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName, gitRepoDb.PersonalAccessTokenSecretName, oldSecretValue));
                }
                throw new LunaServerException(e.Message, innerException: e);
            }
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(GitRepo).Name, gitRepo.RepoName));

            return gitRepoDb;
        }

        /// <summary>
        /// Delete a gitRepo
        /// </summary>
        /// <param name="repoName">The Git Repo name</param>
        /// <returns></returns>
        public async Task<GitRepo> DeleteAsync(string repoName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(GitRepo).Name, repoName));

            var gitRepo = await GetAsync(repoName);

            // Remove the gitRepo from the db
            _context.GitRepos.Remove(gitRepo);
            await _context._SaveChangesAsync();

            // Delete secret from key vault
            if (!string.IsNullOrEmpty(gitRepo.PersonalAccessTokenSecretName))
            {
                try
                {
                    await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, gitRepo.PersonalAccessTokenSecretName));
                }
                catch 
                {
                    // Log the warning and ignore the exception
                    _logger.LogWarning($"Failed to delete secret {gitRepo.PersonalAccessTokenSecretName} while deleting Git repo {gitRepo.RepoName}");
                }
            }

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(GitRepo).Name, repoName));

            return gitRepo;
        }
    }
}
