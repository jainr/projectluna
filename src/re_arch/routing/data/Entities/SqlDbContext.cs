using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Data.Entities
{
    /// <summary>
    /// The SQL database context for Entity framework
    /// </summary>
    public class SqlDbContext : DbContext, ISqlDbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options)
            : base(options)
        { }

        public DbSet<PublishedAPIVersionDB> PublishedAPIVersions { get; set; }

        public DbSet<PartnerServiceDbView> PartnerServices { get; set; }

        /// <summary>
        /// Save changes to database
        /// </summary>
        /// <returns></returns>
        public async Task<int> _SaveChangesAsync()
        {
            return await this.SaveChangesAsync();
        }

        /// <summary>
        /// Begin a transaction in database
        /// </summary>
        /// <returns>The database transaction</returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await this.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Defines the shape of the entities, its relationships
        /// and how they map to the database using the Fluent API.
        /// 
        /// This helps EF Core understand the relationship between
        /// tables with many FKs.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("routing");
        }
    }
}
