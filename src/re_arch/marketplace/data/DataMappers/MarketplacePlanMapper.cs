using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Marketplace.Data
{

    public class MarketplacePlanMapper :
        IDataMapper<MarketplacePlanRequest, MarketplacePlanResponse, MarketplacePlanProp>
    {

        public MarketplacePlanProp Map(MarketplacePlanRequest request)
        {
            MarketplacePlanProp prop = new MarketplacePlanProp
            {
                Description = request.Description,
                DisplayName = request.DisplayName,
                Mode = request.Mode,
                OnSubscribe = request.OnSubscribe,
                OnUpdate = request.OnUpdate,
                OnSuspend = request.OnSuspend,
                OnDelete = request.OnDelete,
                OnPurge = request.OnPurge,
                LunaApplicationName = request.LunaApplicationName,
            };

            return prop;
        }

        public MarketplacePlanResponse Map(MarketplacePlanProp prop)
        {
            MarketplacePlanResponse response = new MarketplacePlanResponse
            {
                Description = prop.Description,
                DisplayName = prop.DisplayName,
                Mode = prop.Mode,
                OnSubscribe = prop.OnSubscribe,
                OnUpdate = prop.OnUpdate,
                OnSuspend = prop.OnSuspend,
                OnDelete = prop.OnDelete,
                OnPurge = prop.OnPurge,
                LunaApplicationName = prop.LunaApplicationName,
            };

            return response;
        }
    }
}
