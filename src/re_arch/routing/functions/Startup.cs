using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Luna.Routing.Clients.MLServiceClients;
using Luna.Routing.Data.Entities;
using Luna.Partner.PublicClient.Clients;
using Luna.Common.Utils.Azure.AzureKeyvaultUtils;
using Luna.PubSub.PublicClient.Clients;
using Luna.Routing.Clients.SecretCacheClients;

[assembly: FunctionsStartup(typeof(Luna.Routing.Functions.Startup))]

namespace Luna.Routing.Functions
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


            builder.Services.AddOptions<PartnerServiceClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("PARTNER_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("PARTNER_SERVICE_KEY");
                });

            builder.Services.AddHttpClient<IPartnerServiceClient, PartnerServiceClient>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            builder.Services.AddHttpClient<IMLServiceClientFactory, MLServiceClientFactory>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            
            // Database context must be registered with the dependency injection (DI) container
            builder.Services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.TryAddScoped<ISqlDbContext, SqlDbContext>();

            builder.Services.AddSingleton<ISecretCacheClient, SecretCacheClient>();

            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
