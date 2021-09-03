using Luna.Common.Utils;
using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Publish.Data
{
    public class LunaApplicationTagMapper : 
        IDataMapper<LunaTagRequest, LunaTagResponse, LunaApplicationTag>
    {
        public LunaApplicationTag Map(LunaTagRequest request)
        {
            return new LunaApplicationTag
            {
                Key = request.Key,
                Value = request.Value
            };
        }

        public LunaTagResponse Map(LunaApplicationTag tag)
        {
            return new LunaTagResponse
            {
                Key = tag.Key,
                Value = tag.Value
            };
        }
    }

    public class LunaApplicationMapper :
        IDataMapper<LunaApplicationRequest, LunaApplicationResponse, LunaApplicationProp>
    {
        private LunaApplicationTagMapper _tagMapper;

        public LunaApplicationMapper()
        {
            this._tagMapper = new LunaApplicationTagMapper();
        }

        public LunaApplicationProp Map(LunaApplicationRequest request)
        {

            var prop = new LunaApplicationProp
            {
                OwnerUserId = request.OwnerUserId,
                DisplayName = request.DisplayName,
                Description = request.Description,
                DocumentationUrl = request.DocumentationUrl,
                LogoImageUrl = request.LogoImageUrl,
                Publisher = request.Publisher,
                Tags = request.Tags.Select(x => this._tagMapper.Map(x)).ToList()
            };

            return prop;
        }

        public LunaApplicationResponse Map(LunaApplicationProp prop)
        {
            var tagMapper = new LunaApplicationTagMapper();

            var response = new LunaApplicationResponse
            {
                OwnerUserId = prop.OwnerUserId,
                DisplayName = prop.DisplayName,
                Description = prop.Description,
                DocumentationUrl = prop.DocumentationUrl,
                LogoImageUrl = prop.LogoImageUrl,
                Publisher = prop.Publisher,
                Tags = prop.Tags.Select(x => this._tagMapper.Map(x)).ToList()
            };

            return response;
        }
    }
}
