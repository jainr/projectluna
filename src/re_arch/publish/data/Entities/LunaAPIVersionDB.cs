using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Publish.Data
{
    public class LunaAPIVersionDB
    {
        public LunaAPIVersionDB()
        {
        }

        public LunaAPIVersionDB(string appName, string apiName, string versionName, string versionType)
        {
            this.ApplicationName = appName;
            this.APIName = apiName;
            this.VersionName = versionName;
            this.VersionType = versionType;
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        public APIVersion GetAPIVersion()
        {
            return new APIVersion(this.VersionName, null);
        }

        public string ApplicationName { get; set; }

        public string APIName { get; set; }

        public string VersionName { get; set; }

        public string VersionType { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
