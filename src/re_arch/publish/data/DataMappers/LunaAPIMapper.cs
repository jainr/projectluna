using Luna.Common.Utils;
using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Publish.Data
{

    public class LunaAPIMapper :
        IDataMapper<BaseLunaAPIRequest, BaseLunaAPIResponse, BaseLunaAPIProp>
    {

        public BaseLunaAPIProp Map(BaseLunaAPIRequest request)
        {
            BaseLunaAPIProp prop = null;

            if (request is RealtimeEndpointAPIRequest)
            {
                prop = new RealtimeEndpointLunaAPIProp
                {
                    Type = request.Type,
                    DisplayName = request.DisplayName,
                    AdvancedSettings = request.AdvancedSettings,
                    Description = request.Description
                };
            }
            else if (request is PipelineEndpointAPIRequest)
            {
                prop = new PipelineEndpointLunaAPIProp
                {
                    Type = request.Type,
                    DisplayName = request.DisplayName,
                    AdvancedSettings = request.AdvancedSettings,
                    Description = request.Description
                };
            }
            else if (request is MLProjectAPIRequest)
            {
                prop = new MLProjectLunaAPIProp
                {
                    Type = request.Type,
                    DisplayName = request.DisplayName,
                    AdvancedSettings = request.AdvancedSettings,
                    Description = request.Description
                };
            }
            else
            {
                throw new LunaServerException($"Unknown Luna API request type {request.GetType().FullName}");
            }

            return prop;
        }

        public BaseLunaAPIResponse Map(BaseLunaAPIProp prop)
        {
            BaseLunaAPIResponse response = null;

            if (prop is RealtimeEndpointLunaAPIProp)
            {
                response = new RealtimeEndpointAPIResponse
                {
                    Type = prop.Type,
                    DisplayName = prop.DisplayName,
                    AdvancedSettings = prop.AdvancedSettings,
                    Description = prop.Description
                };
            }
            else if (prop is PipelineEndpointLunaAPIProp)
            {
                response = new PipelineEndpointAPIResponse
                {
                    Type = prop.Type,
                    DisplayName = prop.DisplayName,
                    AdvancedSettings = prop.AdvancedSettings,
                    Description = prop.Description
                };
            }
            else if (prop is MLProjectLunaAPIProp)
            {
                response = new MLProjectAPIResponse
                {
                    Type = prop.Type,
                    DisplayName = prop.DisplayName,
                    AdvancedSettings = prop.AdvancedSettings,
                    Description = prop.Description
                };
            }
            else
            {
                throw new LunaServerException($"Unknown Luna API property type {prop.GetType().FullName}");
            }

            return response;
        }
    }
}
