using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace Luna.Agent.Data
{
    public interface ISqlDbContext
    {
        DbSet<ProvisioningJobDb> ProvisioningJobs { get; set; }

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
