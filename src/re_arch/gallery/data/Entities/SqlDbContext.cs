using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Gallery.Data
{
    /// <summary>
    /// The SQL database context for Entity framework
    /// </summary>
    public class SqlDbContext : DbContext, ISqlDbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options)
            : base(options)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("USER_ASSIGNED_MANAGED_IDENTITY")))
            {
                var connectionString = @$"RunAs=App;AppId={Environment.GetEnvironmentVariable("USER_ASSIGNED_MANAGED_IDENTITY")}";
                var connection = (SqlConnection)Database.GetDbConnection();
                connection.AccessToken = (new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider(connectionString)).
                    GetAccessTokenAsync("https://database.windows.net/").Result;
            }
        }

        public DbSet<PublishedLunaAppliationDB> PublishedLunaAppliations { get; set; }
        public DbSet<LunaApplicationSubscriptionDB> LunaApplicationSubscriptions { get; set; }
        public DbSet<LunaApplicationSubscriptionOwnerDB> LunaApplicationSubscriptionOwners { get; set; }

        public DbSet<AzureMarketplaceSubscriptionDB> AzureMarketplaceSubscriptions { get; set; }
        public DbSet<PublishedAzureMarketplacePlanDB> PublishedAzureMarketplacePlans { get; set; }

        public DbSet<ApplicationPublisherDB> ApplicationPublishers { get; set; }

        public DbSet<LunaApplicationSwaggerDBView> LunaApplicationSwaggers { get; set; }

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
            modelBuilder.HasDefaultSchema("gallery");

            modelBuilder.Entity<LunaApplicationSubscriptionOwnerDB>()
                .HasOne(o => o.Subscription)
                .WithMany(s => s.Owners);
        }
    }
}
