using Luna.Partner.Public.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Data
{
    /// <summary>
    /// The database entity for partner services
    /// </summary>
    public class PartnerServiceDb
    {
        public PartnerServiceDb()
        {

        }

        public void UpdateFromConfig(BasePartnerServiceConfiguration config)
        {
            this.DisplayName = config.DisplayName;
            this.Description = config.Description;
            this.Tags = config.Tags;
            this.LastUpdatedTime = DateTime.UtcNow;
        }

        public static PartnerServiceDb CreateFromConfig(string name, BasePartnerServiceConfiguration config)
        {
            var service = new PartnerServiceDb();
            // Copy non-updatable fields
            service.UniqueName = name;
            service.Type = config.Type;
            service.Description = config.Description;
            service.Tags = config.Tags;
            service.DisplayName = config.DisplayName;
            service.CreatedTime = DateTime.UtcNow;
            service.LastUpdatedTime = service.CreatedTime;
            service.Configuration = config;

            return service;
        }

        public PartnerServiceOutlineResponse ToPublicPartnerService()
        {
            var service = new PartnerServiceOutlineResponse()
            {
                DisplayName = this.DisplayName,
                UniqueName = this.UniqueName,
                Type = this.Type,
                Description = this.Description,
                Tags = this.Tags,
                CreatedTime = this.CreatedTime,
                LastUpdatedTime = this.LastUpdatedTime
            };

            return service;
        }

        [JsonIgnore]
        public long Id { get; set; }

        public string UniqueName { get; set; }

        public string DisplayName { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        [NotMapped]
        public BasePartnerServiceConfiguration Configuration { get; set; }

        public string ConfigurationSecretName { get; set; }

        public string Tags { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
