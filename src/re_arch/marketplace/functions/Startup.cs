using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Luna.Marketplace.Data;
using Luna.Marketplace.Clients;
using Luna.Common.Utils;
using Luna.PubSub.Public.Client;
using Luna.Marketplace.Public.Client;

[assembly: FunctionsStartup(typeof(Luna.Publish.Functions.Startup))]

namespace Luna.Publish.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<AzureKeyVaultConfiguration>().Configure(
                options =>
                {
                    options.KeyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
                });

            builder.Services.AddHttpClient<IAzureKeyVaultUtils, AzureKeyVaultUtils>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            builder.Services.AddOptions<PubSubServiceClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("PUBSUB_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("PUBSUB_SERVICE_KEY");
                });

            builder.Services.AddSingleton<IPubSubServiceClient, PubSubServiceClient>();
            builder.Services.AddSingleton<IDataMapper<MarketplaceOfferRequest, MarketplaceOfferResponse, MarketplaceOfferProp>, MarketplaceOfferMapper>();
            builder.Services.AddSingleton<IDataMapper<MarketplacePlanRequest, MarketplacePlanResponse, MarketplacePlanProp>, MarketplacePlanMapper>();
            builder.Services.AddSingleton<IDataMapper<MarketplaceParameterRequest, MarketplaceParameterResponse, MarketplaceParameter>, MarketplaceParameterMapper>();
            builder.Services.AddSingleton<IDataMapper<BaseProvisioningStepRequest, BaseProvisioningStepResponse, BaseProvisioningStepProp>, MarketplaceProvisioningStepMapper>();
            builder.Services.AddSingleton<IDataMapper<MarketplaceSubscriptionRequest, MarketplaceSubscriptionResponse, MarketplaceSubscriptionDB>, MarketplaceSubscriptionMapper>();
            builder.Services.AddSingleton<IDataMapper<MarketplaceSubscriptionDB, MarketplaceSubscriptionEventContent>, MarketplaceSubscriptionEventMapper>();
            builder.Services.AddSingleton<IDataMapper<InternalMarketplaceSubscriptionResponse, MarketplaceSubscriptionResponse>, ResolvedMarketplaceSubscriptionMapper>();

            builder.Services.AddSingleton<IOfferEventContentGenerator, OfferEventContentGenerator>();
            builder.Services.AddSingleton<IOfferEventProcessor, OfferEventProcessor>();

            builder.Services.AddOptions<AzureMarketplaceSaaSClientConfiguration>().Configure(
                options =>
                {
                    options.TenantId = Environment.GetEnvironmentVariable("MARKETPLACE_AUTH_TENANT_ID");
                    options.ClientId = Environment.GetEnvironmentVariable("MARKETPLACE_AUTH_CLIENT_ID");
                    options.ClientSecret = Environment.GetEnvironmentVariable("MARKETPLACE_AUTH_CLIENT_SECRET");
                });

            builder.Services.AddHttpClient<IAzureMarketplaceSaaSClient, AzureMarketplaceSaaSClient>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            
            // Database context must be registered with the dependency injection (DI) container
            builder.Services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.TryAddScoped<ISqlDbContext, SqlDbContext>();

            builder.Services.TryAddScoped<IMarketplaceFunctionsImpl, MarketplaceFunctionsImpl>();

            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
