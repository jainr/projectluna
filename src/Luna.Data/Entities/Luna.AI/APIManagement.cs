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
    public partial class APIManagement
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public APIManagement()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="workspace">The object to be copied.</param>
        public void Copy(APIManagement apiMgmt)
        {
        }
    
        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        public string APIManagementName { get; set; }

        public string CertThumbprint { get; set; }

        public string CertIssuer { get; set; }

        public string CertSubject { get; set; }

        public bool AutoPublish { get; set; }

        public string ManagementUrl { get; set; }
        
        public Guid AADApplicationId { get; set; }

        [NotMapped]
        public string AADApplicationSecrets { get; set; }

        [JsonIgnore]
        public string AADApplicationSecretName { get; set; }

        public Guid AADTenantId { get; set; }
    }
}