using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Repository;
using Luna.Clients.Azure.Auth;
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Azure.Storage;
using Luna.Clients.GitUtils;
using Luna.Clients.Azure;
using Luna.Data.Enums;
using Luna.Data.Constants;

namespace Luna.Services.Data.Luna.AI
{
    public class APIVersionService : IAPIVersionService
    {
        private readonly ISqlDbContext _context;
        private readonly IAIServiceService _aiServiceService;
        private readonly IAIServicePlanService _aiServicePlanService;
        private readonly IAMLWorkspaceService _amlWorkspaceService;
        private readonly IAzureDatabricksWorkspaceService _adbWorkspaceService;
        private readonly IGitRepoService _gitRepoService;
        private readonly IAzureSynapseWorkspaceService _synapseWorkspaceService;
        private readonly ILogger<APIVersionService> _logger;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly IStorageUtility _storageUtillity;
        private readonly IGitUtility _gitUtility;
        private readonly IOptionsMonitor<AzureConfigurationOption> _options;

        /// <summary>
        /// Constructor that uses dependency injection.
        /// </summary>
        /// <param name="sqlDbContext">The context to be injected.</param>
        /// <param name="aiServiceService">The service to be injected.</param>
        /// <param name="aiServicePlanService">The service to be injected.</param>
        /// <param name="logger">The logger.</param>
        public APIVersionService(ISqlDbContext sqlDbContext, IAIServiceService aiServiceService, IAIServicePlanService aiServicePlanService, IAMLWorkspaceService amlWorkspaceService,
            IAzureDatabricksWorkspaceService adbWorkspaceService, IGitRepoService gitRepoService, IAzureSynapseWorkspaceService synapseWorkspaceService,
            ILogger<APIVersionService> logger, IKeyVaultHelper keyVaultHelper, IStorageUtility storageUtility, IGitUtility gitUtility, IOptionsMonitor<AzureConfigurationOption> options)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _aiServiceService = aiServiceService ?? throw new ArgumentNullException(nameof(aiServiceService));
            _aiServicePlanService = aiServicePlanService ?? throw new ArgumentNullException(nameof(aiServicePlanService));
            _amlWorkspaceService = amlWorkspaceService ?? throw new ArgumentNullException(nameof(amlWorkspaceService));
            _adbWorkspaceService = adbWorkspaceService ?? throw new ArgumentNullException(nameof(adbWorkspaceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageUtillity = storageUtility ?? throw new ArgumentNullException(nameof(storageUtility));
            _gitUtility = gitUtility ?? throw new ArgumentNullException(nameof(gitUtility));
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _gitRepoService = gitRepoService ?? throw new ArgumentNullException(nameof(gitRepoService));
            _synapseWorkspaceService = synapseWorkspaceService ?? throw new ArgumentNullException(nameof(synapseWorkspaceService));
        }

        /// <summary>
        /// Gets all apiVersions within a aiService and a aiServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the aiService.</param>
        /// <param name="aiServicePlanName">The name of the aiServicePlan.</param>
        /// <returns>A list of apiVersions.</returns>
        public async Task<List<APIVersion>> GetAllAsync(string aiServiceName, string aiServicePlanName)
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(APIVersion).Name));

            // Get the offer associated with the aiServiceName and aiServicePlanName provided
            var aiServicePlan = await _aiServicePlanService.GetAsync(aiServiceName, aiServicePlanName);
            var aiService = await _aiServiceService.GetAsync(aiServiceName);
            
            // Get all apiVersions with a FK to the aiServicePlan
            var apiVersions = await _context.APIVersions.Where(v => v.AIServicePlanId == aiServicePlan.Id).ToListAsync();

            List<APIVersion> result = new List<APIVersion>();

            foreach (var apiVersion in apiVersions)
            {
                result.Add(await FillInInformation(apiVersion, aiService, aiServicePlan));
            }

            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(APIVersion).Name, result.Count()));

            return result;
        }

        private async Task<APIVersion> FillInInformation(APIVersion apiVersion, AIService aiService, AIServicePlan aiServicePlan)
        {
            apiVersion.AIServicePlanName = aiServicePlan.AIServicePlanName;
            apiVersion.AIServiceName = aiService.AIServiceName;

            if (apiVersion.AzureDatabricksWorkspaceId.HasValue)
            {
                var adbWorkspace = await _context.AzureDatabricksWorkspaces.FindAsync(apiVersion.AzureDatabricksWorkspaceId);
                apiVersion.AzureDatabricksWorkspaceName = adbWorkspace.WorkspaceName;
            }
            else if (apiVersion.AMLWorkspaceId.HasValue)
            {
                var amlWorkspace = await _context.AMLWorkspaces.FindAsync(apiVersion.AMLWorkspaceId);
                apiVersion.AMLWorkspaceName = amlWorkspace.WorkspaceName;
            }
            else if (apiVersion.AzureSynapseWorkspaceId.HasValue)
            {
                var synapseWorkspace = await _context.AzureSynapseWorkspaces.FindAsync(apiVersion.AzureSynapseWorkspaceId);
                apiVersion.AzureSynapseWorkspaceName = synapseWorkspace.WorkspaceName;
            }

            if (aiServicePlan.IsModelPlanType())
            {
                var models = await _context.MLModels.Where(v => v.APIVersionId == apiVersion.Id).ToListAsync();

                foreach (MLModel model in models)
                {
                    apiVersion.MLModels.Add(model);
                }
            }
            else if (aiServicePlan.IsEndpointPlanType())
            {
                if (apiVersion.IsManualInputEndpoint)
                {
                    apiVersion.EndpointAuthSecret = LunaConstants.SECRET_NOT_CHANGED_VALUE;
                }
            }
            else if (aiServicePlan.IsPipelinePlanType())
            {

                var pipelineEndpoints = await _context.AMLPipelineEndpoints.Where(v => v.APIVersionId == apiVersion.Id).ToListAsync();

                foreach (AMLPipelineEndpoint pipeline in pipelineEndpoints)
                {
                    apiVersion.AMLPipelineEndpoints.Add(pipeline);
                }
            }
            else if (aiServicePlan.IsMLProjectPlanType())
            {
                if (apiVersion.GitRepoId != null)
                {
                    var gitRepo = await _context.GitRepos.FindAsync(apiVersion.GitRepoId);
                    apiVersion.GitRepoName = gitRepo.RepoName;
                }
            }

            return apiVersion;
        }

        /// <summary>
        /// Gets an apiVersion within a aiService and a aiServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the aiService.</param>
        /// <param name="aiServicePlanName">The name of the aiServicePlan to get.</param>
        /// <param name="versionName">The name of the apiVersion to get.</param>
        /// <returns>The apiVersion.</returns>
        public async Task<APIVersion> GetAsync(string aiServiceName, string aiServicePlanName, string versionName)
        {
            // Check that an apiVersion with the provided versionName exists within the given aiService and aiServicePlan
            if (!(await ExistsAsync(aiServiceName, aiServicePlanName, versionName)))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(APIVersion).Name,
                        versionName));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(APIVersion).Name, versionName));

            // Get the aiServicePlan associated with the aiServiceName and aiServicePlanName provided
            var aiServicePlan = await _aiServicePlanService.GetAsync(aiServiceName, aiServicePlanName);

            // Find the apiVersion that matches the aiServiceName and aiServicePlanName provided
            var apiVersion = await _context.APIVersions
                .SingleOrDefaultAsync(v => (v.AIServicePlanId == aiServicePlan.Id) && (v.VersionName == versionName));
            apiVersion.AIServicePlanName = aiServicePlan.AIServicePlanName;

            // Get the aiService associated with the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);
            apiVersion.AIServiceName = aiService.AIServiceName;

            var result = await FillInInformation(apiVersion, aiService, aiServicePlan);

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(APIVersion).Name,
                versionName,
                JsonSerializer.Serialize(result)));

            return result;
        }

        /// <summary>
        /// Creates an apiVersion within a aiService and a aiServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the aiService.</param>
        /// <param name="aiServicePlanName">The name of the aiServicePlan.</param>
        /// <param name="version">The apiVersion to create.</param>
        /// <returns>The created apiVersion.</returns>
        public async Task<APIVersion> CreateAsync(string aiServiceName, string aiServicePlanName, APIVersion version)
        {
            if (version is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check that the aiService and the aiServicePlan does not already have an apiVersion with the same versionName
            if (await ExistsAsync(aiServiceName, aiServicePlanName, version.VersionName))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(APIVersion).Name,
                    version.VersionName));
            }

            if (ExpressionEvaluationUtils.ReservedParameterNames.Contains(version.VersionName))
            {
                throw new LunaConflictUserException($"Parameter {version.VersionName} is reserved. Please use a different name.");
            }
            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(APIVersion).Name, version.VersionName,
                payload: JsonSerializer.Serialize(version)));

            // Get the aiService associated with the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);

            // Get the aiServicePlan associated with the aiServiceName and the aiServicePlanName provided
            var aiServicePlan = await _aiServicePlanService.GetAsync(aiServiceName, aiServicePlanName);

            // Set the FK to apiVersion
            version.AIServicePlanId = aiServicePlan.Id;

            // Update the apiVersion created time
            version.CreatedTime = DateTime.UtcNow;

            // Update the apiVersion last updated time
            version.LastUpdatedTime = version.CreatedTime;

            version = await ValidateBeforeCreateOrUpdate(aiServicePlan, version);

            // Save secret in key vault

            if (!string.IsNullOrEmpty(version.EndpointAuthSecret))
            {
                string secretName = string.Format(LunaConstants.ENDPOINT_AUTH_SECRET_NAME_FORMAT, Context.GetRandomString(12));
                await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName,
                    secretName, version.EndpointAuthSecret));
                version.EndpointAuthSecretName = secretName;
            }

            using (var transaction = await _context.BeginTransactionAsync())
            {
                // Add apiVersion to db
                _context.APIVersions.Add(version);
                await _context._SaveChangesAsync();
                foreach (var pipeline in version.AMLPipelineEndpoints)
                {
                    pipeline.APIVersionId = version.Id;
                    _context.AMLPipelineEndpoints.Add(pipeline);
                }

                foreach (var model in version.MLModels)
                {
                    model.APIVersionId = version.Id;
                    _context.MLModels.Add(model);
                }
                await _context._SaveChangesAsync();
                transaction.Commit();
            }
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(APIVersion).Name, version.VersionName));

            return version;
        }

        private async Task<APIVersion> ValidateBeforeCreateOrUpdate(AIServicePlan aiServicePlan, APIVersion version, bool isUpdate = false)
        {
            int linkedServiceCount = 0;

            if (version.IsLinkedToADB())
            {
                if (string.IsNullOrEmpty(version.AzureDatabricksWorkspaceName))
                {
                    throw new LunaBadRequestUserException("The Azure Databricks workspace name is required.",
                        UserErrorCode.InvalidParameter);
                }

                var azureDatabricksWorkspace = await _adbWorkspaceService.GetAsync(version.AzureDatabricksWorkspaceName);
                version.AzureDatabricksWorkspaceId = azureDatabricksWorkspace.Id;
                linkedServiceCount++;
            }

            if (version.IsLinkedToAML())
            {
                if (string.IsNullOrEmpty(version.AMLWorkspaceName))
                {
                    throw new LunaBadRequestUserException("The Azure Machine Learning workspace name is required.",
                        UserErrorCode.InvalidParameter);
                }

                var amlWorkspace = await _amlWorkspaceService.GetAsync(version.AMLWorkspaceName);
                version.AMLWorkspaceId = amlWorkspace.Id;
                linkedServiceCount++;
            }

            if (version.IsLinkedToSynapse())
            {
                if (string.IsNullOrEmpty(version.AzureSynapseWorkspaceName))
                {
                    throw new LunaBadRequestUserException("The Azure Synapse workspace name is required.",
                        UserErrorCode.InvalidParameter);
                }

                var synapseWorkspace = await _synapseWorkspaceService.GetAsync(version.AzureSynapseWorkspaceName);
                version.AzureSynapseWorkspaceId = synapseWorkspace.Id;
                linkedServiceCount++;
            }

            // At most one linked service can be selected
            if (linkedServiceCount > 1)
            {
                throw new LunaBadRequestUserException("Multiple linked service provided. At most one linked service supported.",
                    UserErrorCode.InvalidParameter);
            }

            if (aiServicePlan.IsModelPlanType())
            {
                if (!version.IsLinkedToAML() && !version.IsLinkedToADB())
                {
                    throw new LunaBadRequestUserException("Azure Machine Learning service or Azure Databricks (mlflow) is required to publish a model.",
                        UserErrorCode.InvalidParameter);
                }

                if (version.MLModels.Count == 0)
                {
                    throw new LunaBadRequestUserException("At least one model required to publish.",
                        UserErrorCode.InvalidParameter);
                }

                if (version.MLModels.GroupBy(a => a.ModelName).Where(a => a.Count() > 1).Count() > 0)
                {
                    throw new LunaBadRequestUserException("Model names in the same API version must be unique.",
                        UserErrorCode.InvalidParameter);
                }

                // TODO: check if models exist
            }
            else if (aiServicePlan.IsEndpointPlanType())
            {
                if (version.IsManualInputEndpoint)
                {
                    if (string.IsNullOrEmpty(version.EndpointUrl))
                    {
                        throw new LunaBadRequestUserException("Endpoint URL is required when publishing an endpoint manually.",
                            UserErrorCode.InvalidParameter);
                    }

                    if (!version.EndpointUrl.StartsWith("https://"))
                    {
                        throw new LunaBadRequestUserException("Endpoint URL is not a valid https URL.",
                            UserErrorCode.InvalidParameter);
                    }

                    if (!string.IsNullOrEmpty(version.EndpointUrl) && !version.EndpointSwaggerUrl.StartsWith("https://"))
                    {
                        throw new LunaBadRequestUserException("Swagger URL is not a valid https URL.",
                            UserErrorCode.InvalidParameter);
                    }

                    if (string.IsNullOrEmpty(version.EndpointAuthType))
                    {
                        throw new LunaBadRequestUserException("Authentication type is required when publishing an endpoint manually.",
                            UserErrorCode.InvalidParameter);
                    }

                    if (version.EndpointAuthType.Equals(EndpointAuthTypes.API_KEY.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(version.EndpointAuthKey))
                        {
                            throw new LunaBadRequestUserException("The authentication key name is not specified.",
                                UserErrorCode.InvalidParameter);
                        }

                        if (string.IsNullOrEmpty(version.EndpointAuthSecret))
                        {
                            throw new LunaBadRequestUserException("The authentication secret value is not specified.",
                                UserErrorCode.InvalidParameter);
                        }

                        if (string.IsNullOrEmpty(version.EndpointAuthAddTo))
                        {
                            throw new LunaBadRequestUserException("The add to target of API keys is not specified.",
                                UserErrorCode.InvalidParameter);
                        }

                        EndpointAuthAddToTargets target;
                        if (!Enum.TryParse(version.EndpointAuthAddTo, out target))
                        {
                            throw new LunaBadRequestUserException("The add to target value of API keys is is invalid.",
                                UserErrorCode.InvalidParameter);
                        }
                    }
                    else if (version.EndpointAuthType.Equals(EndpointAuthTypes.SERVICE_PRINCIPAL.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (version.EndpointAuthTenantId == null)
                        {
                            throw new LunaBadRequestUserException("The service principal tenant id is not specified or not a valid Guid.",
                                UserErrorCode.InvalidParameter);
                        }

                        if (version.EndpointAuthClientId == null)
                        {
                            throw new LunaBadRequestUserException("The service principal client id is not specified or not a valid Guid.",
                                UserErrorCode.InvalidParameter);
                        }

                        if (string.IsNullOrEmpty(version.EndpointAuthSecret))
                        {
                            throw new LunaBadRequestUserException("The service principal client secret is not specified.",
                                UserErrorCode.InvalidParameter);
                        }
                    }
                    else
                    {
                        throw new LunaBadRequestUserException("The endpoint authentication type is invalid.",
                            UserErrorCode.InvalidParameter);
                    }

                }
                else
                {
                    if (!version.IsLinkedToAML() && !version.IsLinkedToADB())
                    {
                        throw new LunaBadRequestUserException("Azure machine learning service or Azure Databricks workspace is required.",
                            UserErrorCode.InvalidParameter);
                    }
                    if (string.IsNullOrEmpty(version.EndpointName))
                    {
                        throw new LunaBadRequestUserException("Endpoint name is required.",
                            UserErrorCode.InvalidParameter);
                    }
                    if (version.IsLinkedToADB() && string.IsNullOrEmpty(version.EndpointVersion))
                    {
                        throw new LunaBadRequestUserException("Endpoint version is required when publishing from Azure Databricks.",
                            UserErrorCode.InvalidParameter);

                    }
                    // TODO: check if endpoint exists
                    // TODO: cache endpoint info for perf improvement
                }
            }
            else if (aiServicePlan.IsPipelinePlanType())
            {
                if (!version.IsLinkedToAML())
                {
                    throw new LunaBadRequestUserException("Azure Machine Learning service is required to publish pipelines.",
                        UserErrorCode.InvalidParameter);
                }

                foreach (var pipeline in version.AMLPipelineEndpoints)
                {
                    // TODO: check if pipeline exists
                }

                // TODO (Low Pri): cache pipeline endpoint info for perf improvement
            }
            else if (aiServicePlan.IsMLProjectPlanType())
            {
                if (string.IsNullOrEmpty(version.GitRepoName))
                {
                    throw new LunaBadRequestUserException("The Git repo is required when publishing mlflow projects.",
                        UserErrorCode.InvalidParameter);
                }

                var repo = await _gitRepoService.GetAsync(version.GitRepoName);

                version.GitRepoId = repo.Id;

                if (string.IsNullOrEmpty(version.GitVersion))
                {
                    throw new LunaBadRequestUserException("Git commit hash or branch name is required when publishing mlflow projects.",
                        UserErrorCode.InvalidParameter);
                }

                if (!version.IsUseDefaultRunConfig && string.IsNullOrEmpty(version.RunConfigFile))
                {
                    throw new LunaBadRequestUserException("The run config file is required when publishing mlflow projects.",
                        UserErrorCode.InvalidParameter);
                }

                if (version.IsRunProjectOnManagedCompute)
                {
                    if (version.IsLinkedToAML())
                    {
                        if (string.IsNullOrEmpty(version.LinkedServiceComputeTarget))
                        {
                            throw new LunaBadRequestUserException("The compute target is required when running mlflow projects in AML.",
                                UserErrorCode.InvalidParameter);
                        }
                        // TODO: check if compute exist
                    }
                    else if (version.IsLinkedToADB())
                    {
                        // Do nothing for now
                    }
                    else if (version.IsLinkedToSynapse())
                    {
                        if (string.IsNullOrEmpty(version.LinkedServiceComputeTarget))
                        {
                            throw new LunaBadRequestUserException("The compute target is required when running mlflow projects in Azure Synapse.",
                                UserErrorCode.InvalidParameter);
                        }
                    }
                    else
                    {
                        throw new LunaBadRequestUserException("Azure Machine Learning service or Azure Databricks is required to run mlflow projects.",
                            UserErrorCode.InvalidParameter);
                    }
                }
            }
            return version;
        }

        /// <summary>
        /// Updates an apiVersion within a aiService and a aiServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the aiService.</param>
        /// <param name="aiServicePlanName">The name of the aiServicePlan to update.</param>
        /// <param name="versionName">The name of the apiVersion to update.</param>
        /// <param name="version">The updated apiVersion.</param>
        /// <returns>The updated apiVersion.</returns>
        public async Task<APIVersion> UpdateAsync(string aiServiceName, string aiServicePlanName, string versionName, APIVersion version)
        {
            if (version is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            // Check if (the versionName has been updated) && 
            //          (an APIVersion with the same new versionName does not already exist)
            if ((versionName != version.VersionName) && (await ExistsAsync(aiServiceName, aiServicePlanName, version.VersionName)))
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposeNameMismatchErrorMessage(typeof(APIVersion).Name),
                    UserErrorCode.NameMismatch);
            }

            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(APIVersion).Name, versionName, payload: JsonSerializer.Serialize(version)));

            // Get the aiServicePlan associated with the aiServiceName and the aiServicePlanName provided
            var aiServicePlan = await _aiServicePlanService.GetAsync(aiServiceName, aiServicePlanName);

            // Get the apiVersion that matches the aiServiceName, aiServicePlanName and versionName provided
            var versionDb = await GetAsync(aiServiceName, aiServicePlanName, versionName);

            if (versionDb.AIServicePlanId != aiServicePlan.Id)
            {
                throw new LunaNotSupportedUserException("Update the target plan of an existing API version is not supported.");
            }    

            version = await ValidateBeforeCreateOrUpdate(aiServicePlan, version, isUpdate: true);

            // If secret is updated
            if (!string.IsNullOrEmpty(version.EndpointAuthSecret) && !version.EndpointAuthSecret.Equals(LunaConstants.SECRET_NOT_CHANGED_VALUE))
            {
                if (string.IsNullOrEmpty(versionDb.EndpointAuthSecretName))
                {
                    versionDb.EndpointAuthSecretName = string.Format(LunaConstants.ENDPOINT_AUTH_SECRET_NAME_FORMAT, Context.GetRandomString(12));
                }
                await (_keyVaultHelper.SetSecretAsync(_options.CurrentValue.Config.VaultName,
                    versionDb.EndpointAuthSecretName, version.EndpointAuthSecret));
            }

            // Copy over the changes
            versionDb.Copy(version);
            versionDb.LastUpdatedTime = DateTime.UtcNow;

            using (var transaction = await _context.BeginTransactionAsync())
            {
                // Update the AML pipeline list
                // TODO: Consider to improve this using MERGE
                foreach (var pipeline in version.AMLPipelineEndpoints)
                {
                    pipeline.APIVersionId = versionDb.Id;
                    var pipelineDb = await _context.AMLPipelineEndpoints.
                        Where(v => v.APIVersionId == versionDb.Id && v.PipelineEndpointName == pipeline.PipelineEndpointName).
                        SingleOrDefaultAsync();

                    if (pipelineDb != null)
                    {
                        if (pipelineDb.PipelineEndpointId != pipeline.PipelineEndpointId)
                        {
                            pipelineDb.PipelineEndpointId = pipeline.PipelineEndpointId;
                            _context.AMLPipelineEndpoints.Update(pipelineDb);
                        }
                    }
                    else
                    {
                        _context.AMLPipelineEndpoints.Add(pipeline);
                    }
                }
                await _context._SaveChangesAsync();

                var pipelines = await _context.AMLPipelineEndpoints.Where(v => v.APIVersionId == versionDb.Id).ToListAsync();
                foreach (var pipeline in pipelines)
                {
                    if (!version.AMLPipelineEndpoints.Exists(v => v.PipelineEndpointName == pipeline.PipelineEndpointName))
                    {
                        _context.AMLPipelineEndpoints.Remove(pipeline);
                    }
                }
                await _context._SaveChangesAsync();

                // Update the ML model list
                // TODO: Consider to improve this using MERGE
                foreach (var model in version.MLModels)
                {
                    model.APIVersionId = versionDb.Id;
                    var modelDb = await _context.MLModels.
                        Where(v => v.APIVersionId == versionDb.Id && v.ModelName == model.ModelName).
                        SingleOrDefaultAsync();

                    if (modelDb != null)
                    {
                        if (modelDb.ModelVersion != model.ModelVersion || 
                            !modelDb.ModelAlternativeName.Equals(model.ModelAlternativeName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            modelDb.ModelVersion = model.ModelVersion;
                            modelDb.ModelAlternativeName = model.ModelAlternativeName;
                            _context.MLModels.Update(modelDb);
                        }
                    }
                    else
                    {
                        _context.MLModels.Add(model);
                    }
                }
                await _context._SaveChangesAsync();

                var models = await _context.MLModels.Where(v => v.APIVersionId == versionDb.Id).ToListAsync();
                foreach (var model in models)
                {
                    if (!version.MLModels.Exists(v => v.ModelName == model.ModelName))
                    {
                        _context.MLModels.Remove(model);
                    }
                }
                await _context._SaveChangesAsync();

                _context.APIVersions.Update(versionDb);
                await _context._SaveChangesAsync();
                transaction.Commit();
            }

            // Update version values and save changes in db
            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(APIVersion).Name, versionName));

            return version;
        }

        /// <summary>
        /// Deletes an apiVersion within a aiService and a aiServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the aiService.</param>
        /// <param name="aiServicePlanName">The name of the aiServicePlan.</param>
        /// <param name="versionName">The name of the apiVersion to delete.</param>
        /// <returns>The deleted apiVersion.</returns>
        public async Task<APIVersion> DeleteAsync(string aiServiceName, string aiServicePlanName, string versionName)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(APIVersion).Name, versionName));

            // Get the aiService that matches the aiServiceName provided
            var aiService = await _aiServiceService.GetAsync(aiServiceName);

            // Get the apiVersion that matches the aiServiceName, the aiServicePlanName and the versionName provided
            var version = await GetAsync(aiServiceName, aiServicePlanName, versionName);
            version.AIServiceName = aiServiceName;
            version.AIServicePlanName = aiServicePlanName;

            // delete athentication key from keyVault if authentication type is key
            if (!string.IsNullOrEmpty(version.EndpointAuthSecretName))
            {
                string secretName = version.EndpointAuthSecretName;
                try
                {
                    await (_keyVaultHelper.DeleteSecretAsync(_options.CurrentValue.Config.VaultName, secretName));
                }
                catch
                {
                }
            }

            using (var transaction = await _context.BeginTransactionAsync())
            {
                // Remove all pipelines linked to the current API version
                var pipelines = await _context.AMLPipelineEndpoints.Where(v => v.APIVersionId == version.Id).ToListAsync();
                _context.AMLPipelineEndpoints.RemoveRange(pipelines);
                await _context._SaveChangesAsync();

                var models = await _context.MLModels.Where(v => v.APIVersionId == version.Id).ToListAsync();
                _context.MLModels.RemoveRange(models);
                await _context._SaveChangesAsync();

                // Remove the apiVersion from the db
                _context.APIVersions.Remove(version);
                await _context._SaveChangesAsync();
                transaction.Commit();
            }
            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(APIVersion).Name, versionName));

            return version;
        }

        /// <summary>
        /// Checks if an apiVersion exists within a aiService and a aiServicePlan.
        /// </summary>
        /// <param name="aiServiceName">The name of the aiService.</param>
        /// <param name="aiServicePlanName">The name of the aiServicePlan to check exists.</param>
        /// <param name="versionName">The name of the apiVersion to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string aiServiceName, string aiServicePlanName, string versionName)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(APIVersion).Name, versionName));

            //Get the aiServicePlan associated with the aiServiceName and the aiServicePlanName provided
            var aiServicePlan = await _aiServicePlanService.GetAsync(aiServiceName, aiServicePlanName);

            // Check that only one apiVersion with this versionName exists within the aiServicePlan
            var count = await _context.APIVersions
                .CountAsync(a => (a.AIServicePlanId.Equals(aiServicePlan.Id)) && (a.VersionName == versionName));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(APIVersion).Name,
                    versionName));
            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(APIVersion).Name, versionName, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(APIVersion).Name, versionName, true));
                // count = 1
                return true;
            }
        }
    }
}
