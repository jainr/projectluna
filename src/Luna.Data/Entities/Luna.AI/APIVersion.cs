using Luna.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the offers table in the database.
    /// </summary>
    public partial class APIVersion
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public APIVersion()
        {
            IsManualInputEndpoint = false;
            AMLPipelineEndpoints = new List<AMLPipelineEndpoint>();
            MLModels = new List<MLModel>();
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="version">The object to be copied.</param>
        public void Copy(APIVersion version)
        {
            this.ApplicationName = version.ApplicationName;
            this.APIName = version.APIName;
            this.CreatedTime = version.CreatedTime;
            this.GitRepoId = version.GitRepoId;
            this.AMLWorkspaceId = version.AMLWorkspaceId;
            this.AzureDatabricksWorkspaceId = version.AzureDatabricksWorkspaceId;
            this.AzureSynapseWorkspaceId = version.AzureSynapseWorkspaceId;
            this.EndpointAuthAddTo = version.EndpointAuthAddTo;
            this.EndpointAuthClientId = version.EndpointAuthClientId;
            this.EndpointAuthKey = version.EndpointAuthKey;
            this.EndpointAuthTenantId = version.EndpointAuthTenantId;
            this.EndpointAuthType = version.EndpointAuthType;
            this.EndpointName = version.EndpointName;
            this.EndpointVersion = version.EndpointVersion;
            this.EndpointSwaggerUrl = version.EndpointSwaggerUrl;
            this.EndpointUrl = version.EndpointUrl;
            this.GitVersion = version.GitVersion;
            this.IsManualInputEndpoint = version.IsManualInputEndpoint;
            this.IsRunProjectOnManagedCompute = version.IsRunProjectOnManagedCompute;
            this.IsUseDefaultRunConfig = version.IsUseDefaultRunConfig;
            this.LinkedServiceComputeTarget = version.LinkedServiceComputeTarget;
            this.LinkedServiceType = version.LinkedServiceType;
            this.RunConfigFile = version.RunConfigFile;
            this.DataShareAccountName = version.DataShareAccountName;
            this.DataShareName = version.DataShareName;

        }

        public string GetVersionIdFormat()
        {
            return VersionName.Replace(".", "-");
        }

        public bool IsLinkedToAML()
        {
            return !string.IsNullOrEmpty(this.LinkedServiceType) &&
                this.LinkedServiceType.Equals(LinkedServiceTypes.AML.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                !string.IsNullOrEmpty(this.AMLWorkspaceName);
        }
        public bool IsLinkedToADB()
        {
            return !string.IsNullOrEmpty(this.LinkedServiceType) &&
                this.LinkedServiceType.Equals(LinkedServiceTypes.ADB.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                !string.IsNullOrEmpty(this.AzureDatabricksWorkspaceName);
        }
        public bool IsLinkedToSynapse()
        {
            return !string.IsNullOrEmpty(this.LinkedServiceType) &&
                this.LinkedServiceType.Equals(LinkedServiceTypes.Synapse.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                !string.IsNullOrEmpty(this.AzureSynapseWorkspaceName);
        }

        [Key]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public long LunaAPIId { get; set; }
        [NotMapped]
        public string ApplicationName { get; set; }
        [NotMapped]
        public string APIName { get; set; }

        public string VersionName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        // Linked Services
        [JsonIgnore]
        public long? AMLWorkspaceId { get; set; }

        [NotMapped]
        public string AMLWorkspaceName { get; set; }

        [JsonIgnore]
        public long? AzureDatabricksWorkspaceId { get; set; }

        [NotMapped]
        public string AzureDatabricksWorkspaceName { get; set; }

        [JsonIgnore]
        public long? AzureSynapseWorkspaceId { get; set; }

        [NotMapped]
        public string AzureSynapseWorkspaceName { get; set; }

        [JsonIgnore]
        public long? GitRepoId { get; set; }

        [NotMapped]
        public string GitRepoName { get; set; }

        // For AML and ADB only
        public string EndpointName { get; set; }

        public string EndpointVersion { get; set; }

        public bool IsManualInputEndpoint { get; set; }

        // For manual only
        public string EndpointUrl { get; set; }

        public string EndpointSwaggerUrl { get; set; }

        public string EndpointAuthType { get; set; }

        public string EndpointAuthKey { get; set; }

        public string EndpointAuthAddTo { get; set; }

        [NotMapped]
        public string EndpointAuthSecret { get; set; }

        [JsonIgnore]
        public string EndpointAuthSecretName { get; set; }

        public Guid? EndpointAuthTenantId { get; set; }

        public Guid? EndpointAuthClientId { get; set; }

        // Fields for ml project deployment
        public string GitVersion { get; set; }

        [NotMapped]
        public string ModelName { get; set; }

        [NotMapped]
        public string ModelVersion { get; set; }

        [NotMapped]
        public string ModelDisplayName { get; set; }

        [NotMapped]
        public List<MLModel> MLModels { get; set; }

        [NotMapped]
        public List<AMLPipelineEndpoint> AMLPipelineEndpoints { get; set; }

        // Support AML and ADB
        public string LinkedServiceType { get; set; }

        // Support AML and ADB
        public string RunConfigFile { get; set; }

        public bool IsUseDefaultRunConfig { get; set; }

        public bool IsRunProjectOnManagedCompute { get; set; }

        public string LinkedServiceComputeTarget { get; set; }
        public string DataShareAccountName { get; set; }
        public string DataShareName { get; set; }
    }
}