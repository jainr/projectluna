using Luna.Publish.Public.Client.DataContract;
using Luna.Publish.PublicClient.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Luna.Publish.Data.Entities
{
    public class LunaAPIDB
    {
        public LunaAPIDB()
        {
        }

        public LunaAPIDB(string appName, string apiName, string apiType)
        {
            this.ApplicationName = appName;
            this.APIName = apiName;
            this.APIType = apiType;
            this.CreatedTime = DateTime.UtcNow;
            this.LastUpdatedTime = this.CreatedTime;
        }

        public LunaAPI GetLunaAPI()
        {
            return new LunaAPI(this.APIName, null);
        }

        public string ApplicationName { get; set; }

        public string APIName { get; set; }

        public string APIType { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
