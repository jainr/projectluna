using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Gallery.Data
{
    /// <summary>
    /// ISqlDbContext interface for EntityFramework
    /// </summary>
    public interface ISqlDbContext
    {

        DbSet<PublishedLunaAppliationDB> PublishedLunaAppliations { get; set; }
        DbSet<LunaApplicationSubscriptionDB> LunaApplicationSubscriptions { get; set; }

        DbSet<LunaApplicationSubscriptionOwnerDB> LunaApplicationSubscriptionOwners { get; set; }

        DbSet<AzureMarketplaceSubscriptionDB> AzureMarketplaceSubscriptions { get; set; }

        DbSet<PublishedAzureMarketplacePlanDB> PublishedAzureMarketplacePlans { get; set; }

        DbSet<ApplicationPublisherDB> ApplicationPublishers { get; set; }

        DbSet<LunaApplicationSwaggerDBView> LunaApplicationSwaggers { get; set; }

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
