using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Luna.Partner.Clients.PartnerServiceClients;
using Luna.Partner.Data.Entities;
using Luna.Common.Utils.Azure.AzureKeyvaultUtils;
using Luna.Common.Utils;

[assembly: FunctionsStartup(typeof(Luna.RBAC.Functions.Startup))]

namespace Luna.RBAC.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<EncryptionConfiguration>().Configure(
                options =>
                {
                    options.SymmetricKey = Environment.GetEnvironmentVariable("ENCRYPTION_ASYMMETRIC_KEY");
                });

            builder.Services.AddSingleton<IEncryptionUtils, EncryptionUtils>();

            builder.Services.AddOptions<AzureKeyVaultConfiguration>().Configure(
                options =>
                {
                    options.KeyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
                });

            builder.Services.AddHttpClient<IAzureKeyVaultUtils, AzureKeyVaultUtils>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            builder.Services.AddHttpClient<IPartnerServiceClientFactory, PartnerServiceClientFactory>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            
            // Database context must be registered with the dependency injection (DI) container
            builder.Services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.TryAddScoped<ISqlDbContext, SqlDbContext>();
            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
