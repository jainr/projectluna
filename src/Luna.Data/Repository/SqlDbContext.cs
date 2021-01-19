using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Data.Entities.Luna.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Luna.Data.Repository
{
    /// <summary>
    /// Represents a session with the database and can be used to query and 
    /// save instances of your entities.
    /// </summary>
    public class SqlDbContext : DbContext, ISqlDbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options)
            : base(options)
        { }

        public DbSet<AadSecretTmp> AadSecretTmps { get; set; }
        public DbSet<ArmTemplate> ArmTemplates { get; set; }
        public DbSet<ArmTemplateParameter> ArmTemplateParameters { get; set; }
        public DbSet<CustomMeter> CustomMeters { get; set; }
        public DbSet<CustomMeterDimension> CustomMeterDimensions { get; set; }
        public DbSet<IpAddress> IpAddresses { get; set; }
        public DbSet<IpBlock> IpBlocks { get; set; }
        public DbSet<IpConfig> IpConfigs { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OfferParameter> OfferParameters { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<RestrictedUser> RestrictedUsers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Webhook> Webhooks { get; set; }
        public DbSet<WebhookParameter> WebhookParameters { get; set; }
        public DbSet<SubscriptionParameter> SubscriptionParameters { get; set; }
        public DbSet<ArmTemplateArmTemplateParameter> ArmTemplateArmTemplateParameters { get; set; }
        public DbSet<WebhookWebhookParameter> WebhookWebhookParameters { get; set; }
        public DbSet<SubscriptionCustomMeterUsage> SubscriptionCustomMeterUsages { get; set; }
        public DbSet<TelemetryDataConnector> TelemetryDataConnectors { get; set; }
        public DbSet<AIService> AIServices { get; set; }
        public DbSet<AIServicePlan> AIServicePlans { get; set; }
        public DbSet<APIVersion> APIVersions { get; set; }
        public DbSet<AMLWorkspace> AMLWorkspaces { get; set; }
        public DbSet<APISubscription> APISubscriptions { get; set; }
        public DbSet<Gateway> Gateways { get; set; }

        public DbSet<AgentSubscription> AgentSubscriptions { get; set; }
        public DbSet<AgentAPIVersion> AgentAPIVersions { get; set; }

        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<AgentOffer> AgentOffers { get; set; }

        public DbSet<AzureSynapseWorkspace> AzureSynapseWorkspaces { get; set; }

        public DbSet<AzureDatabricksWorkspace> AzureDatabricksWorkspaces { get; set; }

        public DbSet<GitRepo> GitRepos { get; set; }

        public DbSet<AMLPipelineEndpoint> AMLPipelineEndpoints { get; set; }
        public DbSet<MLModel> MLModels { get; set; }
        public DbSet<AIServicePlanGateway> AIServicePlanGateways { get; set; }

        // Wrappers for DbContext methods that are used
        public async Task<int> _SaveChangesAsync()
        {
            return await this.SaveChangesAsync();
        }
        
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await this.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Defines the shape of the Plan entity, its relationships
        /// and how they map to the database using the Fluent API.
        /// 
        /// This helps EF Core understand the relationship between
        /// tables with many FKs.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AgentSubscription>().HasNoKey();

            modelBuilder.Entity<AgentAPIVersion>().HasNoKey();

            modelBuilder.Entity<AgentOffer>().HasNoKey();

            modelBuilder.Entity<ArmTemplateArmTemplateParameter>()
                .HasKey(x => new { x.ArmTemplateId, x.ArmTemplateParameterId });

            modelBuilder.Entity<WebhookWebhookParameter>()
                    .HasKey(x => new { x.WebhookId, x.WebhookParameterId });

            modelBuilder.Entity<AIServicePlanGateway>()
                .HasKey(x => new { x.AIServicePlanId, x.GatewayId });

            modelBuilder.Entity<Plan>(plan =>
            {
                plan.HasOne(o => o.Offer)
                    .WithMany(p => p.Plans)
                    .HasForeignKey(fk => fk.OfferId)
                    .HasConstraintName("FK_offer_id_plans");

                plan.HasOne(a => a.SubscribeArmTemplate)
                    .WithMany(a => a.SubscribeArmTemplateNav)
                    .HasForeignKey(fk => fk.SubscribeArmTemplateId)
                    .HasConstraintName("FK_subscribeArmTemplateId_plans");

                plan.HasOne(a => a.UnsubscribeArmTemplate)
                    .WithMany(a => a.UnsubscribeArmTemplateNav)
                    .HasForeignKey(fk => fk.UnsubscribeArmTemplateId)
                    .HasConstraintName("FK_unsubscribeArmTemplateId_plans");

                plan.HasOne(a => a.SuspendArmTemplate)
                    .WithMany(a => a.SuspendArmTemplateNav)
                    .HasForeignKey(fk => fk.SuspendArmTemplateId)
                    .HasConstraintName("FK_suspendArmTemplateId_plans");

                plan.HasOne(a => a.DeleteDataArmTemplate)
                    .WithMany(a => a.DeleteDataArmTemplateNav)
                    .HasForeignKey(fk => fk.DeleteDataArmTemplateId)
                    .HasConstraintName("FK_deleteDataArmTemplateId_plans");

                plan.HasOne(a => a.SubscribeWebhook)
                    .WithMany(a => a.SubscribeWebhookNav)
                    .HasForeignKey(fk => fk.SubscribeWebhookId)
                    .HasConstraintName("FK_subscribeWebhookId_plans");

                plan.HasOne(a => a.UnsubscribeWebhook)
                    .WithMany(a => a.UnsubscribeWebhookNav)
                    .HasForeignKey(fk => fk.UnsubscribeWebhookId)
                    .HasConstraintName("FK_unsubscribeWebhookId_plans");

                plan.HasOne(a => a.SuspendWebhook)
                    .WithMany(a => a.SuspendWebhookNav)
                    .HasForeignKey(fk => fk.SuspendWebhookId)
                    .HasConstraintName("FK_suspendWebhookId_plans");

                plan.HasOne(a => a.DeleteDataWebhook)
                    .WithMany(a => a.DeleteDataWebhookNav)
                    .HasForeignKey(fk => fk.DeleteDataWebhookId)
                    .HasConstraintName("FK_deleteDataWebhookId_plans");
            });

            modelBuilder.Entity<Subscription>(subscription =>
            {
                subscription.HasOne(o => o.Offer)
                    .WithMany(s => s.Subscriptions)
                    .HasForeignKey(fk => fk.OfferId)
                    .HasConstraintName("FK_offer_id_subscriptions");

                subscription.HasOne(p => p.Plan)
                    .WithMany(s => s.Subscriptions)
                    .HasForeignKey(fk => fk.PlanId)
                    .HasConstraintName("FK_plan_id_subscriptions");
            });
        }
    }
}