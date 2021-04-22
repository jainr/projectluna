using Luna.Partner.PublicClient.DataContract;
using Luna.Partner.PublicClient.DataContract.PartnerServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Data.Entities
{
    /// <summary>
    /// The database entity for partner services
    /// </summary>
    public class PartnerServiceInternal
    {
        public PartnerServiceInternal()
        {

        }

        public static PartnerServiceInternal CreateFrom(PartnerService partnerService)
        {
            var service = new PartnerServiceInternal();
            // Copy non-updatable fields
            service.UniqueName = partnerService.UniqueName;
            service.Type = partnerService.Type;
            service.CreatedTime = DateTime.UtcNow;
            service.LastUpdatedTime = service.CreatedTime;

            service.CopyFrom(partnerService);
            return service;
        }

        public void CopyFrom(PartnerService partnerService)
        {
            // Copy updatable fileds
            this.DisplayName = partnerService.DisplayName;
            this.Description = partnerService.Description;
            this.Configuration = partnerService.Configuration;
            this.Tags = partnerService.Tags;
            this.LastUpdatedTime = DateTime.UtcNow;
        }

        public PartnerService ToPublicCopy(string configuration)
        {
            var service = new PartnerService()
            {
                UniqueName = this.UniqueName,
                DisplayName = this.DisplayName,
                Description = this.Description,
                Type = this.Type,
                Tags = this.Tags,
                CreatedTime = this.CreatedTime,
                LastUpdatedTime = this.LastUpdatedTime
            };

            if(configuration != null)
            {
                service.Configuration = JsonConvert.DeserializeObject<BasePartnerServiceConfiguration>(configuration,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
            }

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
