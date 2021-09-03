using Luna.Common.Utils;
using Luna.Partner.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Partner.Data.DataMappers
{
    public class PartnerServiceMapper :
        IDataMapper<BasePartnerServiceConfiguration, PartnerServiceOutlineResponse, PartnerServiceDb>
    {
        public PartnerServiceDb Map(BasePartnerServiceConfiguration source)
        {
            var service = new PartnerServiceDb();

            service.Type = source.Type;
            service.Description = source.Description;
            service.Tags = source.Tags;
            service.DisplayName = source.DisplayName;
            service.CreatedTime = DateTime.UtcNow;
            service.LastUpdatedTime = service.CreatedTime;
            service.Configuration = source;

            return service;
        }

        public PartnerServiceOutlineResponse Map(PartnerServiceDb dbEntity)
        {
            var service = new PartnerServiceOutlineResponse()
            {
                DisplayName = dbEntity.DisplayName,
                UniqueName = dbEntity.UniqueName,
                Type = dbEntity.Type,
                Description = dbEntity.Description,
                Tags = dbEntity.Tags,
                CreatedTime = dbEntity.CreatedTime,
                LastUpdatedTime = dbEntity.LastUpdatedTime
            };

            return service;
        }
    }
}
