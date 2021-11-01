using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Agent.Data
{
    public class MemoryContext : ISqlDbContext
    {
        public DbSet<ProvisioningJobDb> ProvisioningJobs { get; set; }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<int> _SaveChangesAsync()
        {
            return 1;
        }
    }
}
