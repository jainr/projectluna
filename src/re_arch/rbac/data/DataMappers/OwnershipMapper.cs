using Luna.Common.Utils;
using Luna.RBAC.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Data
{
    public class OwnershipMapper : 
        IDataMapper<OwnershipRequest, OwnershipResponse, OwnershipDb>
    {
        public OwnershipDb Map(OwnershipRequest request)
        {
            return new OwnershipDb
            {
                Uid = request.Uid,
                ResourceId = request.ResourceId,
            };
        }

        public OwnershipResponse Map(OwnershipDb ownership)
        {
            return new OwnershipResponse
            {
                Uid = ownership.Uid,
                ResourceId = ownership.ResourceId,
                CreatedTime = ownership.CreatedTime
            };
        }
    }
}
