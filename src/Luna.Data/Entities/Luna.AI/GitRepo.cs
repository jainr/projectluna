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
    public partial class GitRepo
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public GitRepo()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="repo">The object to be copied.</param>
        public void Copy(GitRepo repo)
        {
            HttpUrl = repo.HttpUrl;
            CommitHashOrBranch = repo.CommitHashOrBranch;
        }
    
        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        public string Type { get; set; }

        public string RepoName { get; set; }

        public string HttpUrl { get; set; }

        public string CommitHashOrBranch { get; set; }
        
        [NotMapped]
        public string PersonalAccessToken { get; set; }

        [JsonIgnore]
        public string PersonalAccessTokenSecretName { get; set; }

    }
}