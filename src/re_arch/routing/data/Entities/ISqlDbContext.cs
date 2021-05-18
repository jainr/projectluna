using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Routing.Data.Entities
{
    /// <summary>
    /// ISqlDbContext interface for EntityFramework
    /// </summary>
    public interface ISqlDbContext
    {

        DbSet<PublishedAPIVersionDB> PublishedAPIVersions { get; set; }

        DbSet<PartnerServiceDbView> PartnerServices { get; set; }

        DbSet<SubscriptionsDBView> Subscriptions { get; set; }

        DbSet<ProcessedEventDB> ProcessedEvents { get; set; }

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
