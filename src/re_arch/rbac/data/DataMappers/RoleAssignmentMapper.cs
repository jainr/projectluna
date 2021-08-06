using Luna.Common.Utils;
using Luna.RBAC.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.RBAC.Data
{
    public class RoleAssignmentMapper :
        IDataMapper<RoleAssignmentRequest, RoleAssignmentResponse, RoleAssignmentDb>
    {
        public RoleAssignmentResponse Map(RoleAssignmentDb roleAssignment)
        {
            return new RoleAssignmentResponse
            {
                Uid = roleAssignment.Uid,
                Role = roleAssignment.Role,
                UserName = roleAssignment.UserName,
                CreatedTime = roleAssignment.CreatedTime
            };
        }

        public RoleAssignmentDb Map(RoleAssignmentRequest request)
        {
            return new RoleAssignmentDb
            {
                Uid = request.Uid,
                Role = request.Role,
                UserName = request.UserName
            };
        }
    }
}
