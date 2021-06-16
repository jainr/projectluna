using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Publish.Data
{
    public class LunaApplicationDB
    {
        public LunaApplicationDB()
        {

        }

        public LunaApplicationDB(string name, 
            string ownerUserId,
            string primaryMasterKeySecretName, 
            string secondaryMasterKeySecretName)
        {
            this.ApplicationName = name;
            this.OwnerUserId = ownerUserId;
            this.PrimaryMasterKeySecretName = primaryMasterKeySecretName;
            this.SecondaryMasterKeySecretName = secondaryMasterKeySecretName;
            this.Status = ApplicationStatus.Draft.ToString();
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        public LunaApplication GetLunaApplication()
        {
            return new LunaApplication(this.ApplicationName, this.Status, null);
        }

        public string ApplicationName { get; set; }

        public string Status { get; set; }

        public string OwnerUserId { get; set; }

        public string PrimaryMasterKeySecretName { get; set; }

        public string SecondaryMasterKeySecretName { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? LastPublishedTime { get; set; }
    }
}
