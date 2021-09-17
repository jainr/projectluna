using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Marketplace.Data
{
    /// <summary>
    /// ISqlDbContext interface for EntityFramework
    /// </summary>
    public interface ISqlDbContext
    {
        DbSet<MarketplaceOfferSnapshotDB> MarketplaceOfferSnapshots { get; set; }

        DbSet<MarketplaceEventDB> MarketplaceEvents { get; set; }

        DbSet<MarketplaceOfferDB> MarketplaceOffers { get; set; }

        DbSet<MarketplacePlanDB> MarketplacePlans { get; set; }

        DbSet<MarketplaceParameterDB> MarketplaceParameters { get; set; }

        DbSet<MarketplaceProvisioningStepDB> MarketplaceProvisioningSteps { get; set; }

        DbSet<MarketplaceSubscriptionDB> MarketplaceSubscriptions { get; set; }

        DbSet<PublishedAzureMarketplacePlanDB> PublishedAzureMarketplacePlans { get; set; }

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
