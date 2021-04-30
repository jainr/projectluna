using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.RBAC.Data.Entities
{
    /// <summary>
    /// ISqlDbContext interface for EntityFramework
    /// </summary>
    public interface ISqlDbContext
    {

        DbSet<RoleAssignmentDb> RoleAssignments { get; set; }
        DbSet<OwnershipDb> Ownerships { get; set; }

        /// <summary>
        /// Save the changes to database
        /// </summary>
        /// <returns></returns>
        Task<int> _SaveChangesAsync();

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        /// <returns>The database transaction</returns>
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
