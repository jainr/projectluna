using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Marketplace.Data
{

    public class MarketplaceParameterMapper :
        IDataMapper<MarketplaceParameterRequest, MarketplaceParameterResponse, MarketplaceParameter>
    {

        public MarketplaceParameter Map(MarketplaceParameterRequest request)
        {
            MarketplaceParameter prop = new MarketplaceParameter
            {
                ParameterName = request.ParameterName,
                DisplayName = request.DisplayName,
                Description = request.Description,
                ValueType = request.ValueType,
                FromList = request.FromList,
                ValueList = request.ValueList,
                Maximum = request.Maximum,
                Minimum = request.Minimum,
                IsRequired = request.IsRequired,
                IsUserInput = request.IsUserInput,
                DefaultValue = request.DefaultValue
            };

            return prop;
        }

        public MarketplaceParameterResponse Map(MarketplaceParameter param)
        {
            MarketplaceParameterResponse response = new MarketplaceParameterResponse
            {
                ParameterName = param.ParameterName,
                DisplayName = param.DisplayName,
                Description = param.Description,
                ValueType = param.ValueType,
                FromList = param.FromList,
                ValueList = param.ValueList,
                Maximum = param.Maximum,
                Minimum = param.Minimum,
                IsRequired = param.IsRequired,
                IsUserInput = param.IsUserInput,
                DefaultValue = param.DefaultValue
            };

            return response;
        }
    }
}
