using System.Threading.Tasks;
using Luna.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Luna.Data.Repository
{
    public interface ISqlDbContext
    {
        DbSet<AadSecretTmp> AadSecretTmps { get; set; }
        DbSet<ArmTemplateParameter> ArmTemplateParameters { get; set; }
        DbSet<ArmTemplate> ArmTemplates { get; set; }
        DbSet<CustomMeterDimension> CustomMeterDimensions { get; set; }
        DbSet<CustomMeter> CustomMeters { get; set; }
        DbSet<IpAddress> IpAddresses { get; set; }
        DbSet<IpBlock> IpBlocks { get; set; }
        DbSet<IpConfig> IpConfigs { get; set; } 
        DbSet<OfferParameter> OfferParameters { get; set; }
        DbSet<Offer> Offers { get; set; }
        DbSet<Plan> Plans { get; set; }
        DbSet<RestrictedUser> RestrictedUsers { get; set; }
        DbSet<Subscription> Subscriptions { get; set; }
        DbSet<Webhook> Webhooks { get; set; }
        DbSet<WebhookParameter> WebhookParameters { get; set; }
        DbSet<SubscriptionParameter> SubscriptionParameters { get; set; }
        DbSet<ArmTemplateArmTemplateParameter> ArmTemplateArmTemplateParameters { get; set; }
        DbSet<WebhookWebhookParameter> WebhookWebhookParameters { get; set; }

        DbSet<SubscriptionCustomMeterUsage> SubscriptionCustomMeterUsages { get; set; }

        DbSet<TelemetryDataConnector> TelemetryDataConnectors { get; set; }

        DbSet<LunaApplication> LunaApplications { get; set; }
        DbSet<LunaAPI> LunaAPIs { get; set; }
        DbSet<APIVersion> APIVersions { get; set; }
        DbSet<AMLWorkspace> AMLWorkspaces { get; set; }

        DbSet<Gateway> Gateways { get; set; }

        DbSet<AgentSubscription> AgentSubscriptions { get; set; }

        DbSet<AgentAPIVersion> AgentAPIVersions { get; set; }

        DbSet<AgentOffer> AgentOffers { get; set; }

        DbSet<AzureSynapseWorkspace> AzureSynapseWorkspaces { get; set; }

        DbSet<AzureDatabricksWorkspace> AzureDatabricksWorkspaces { get; set; }

        DbSet<GitRepo> GitRepos { get; set; }

        DbSet<AMLPipelineEndpoint> AMLPipelineEndpoints { get; set; }
        DbSet<MLModel> MLModels { get; set; }

        DbSet<PlanGateway> PlanGateways { get; set; }
        DbSet<PlanApplication> PlanApplications { get; set; }

        // Wrappers for DbContext methods that are used
        Task<int> _SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}