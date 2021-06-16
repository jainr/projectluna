using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Publish.Data
{
    /// <summary>
    /// ISqlDbContext interface for EntityFramework
    /// </summary>
    public interface ISqlDbContext
    {

        DbSet<ApplicationSnapshotDB> ApplicationSnapshots { get; set; }

        DbSet<PublishingEventDB> PublishingEvents { get; set; }

        DbSet<LunaApplicationDB> LunaApplications { get; set; }

        DbSet<LunaAPIDB> LunaAPIs { get; set; }

        DbSet<LunaAPIVersionDB> LunaAPIVersions { get; set; }

        DbSet<AzureMarketplaceOfferDB> AzureMarketplaceOffers { get; set; }

        DbSet<AzureMarketplacePlanDB> AzureMarketplacePlans { get; set; }

        DbSet<AutomationWebhookDB> AutomationWebhooks { get; set; }

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
