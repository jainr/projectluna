using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Luna.Publish.Data;
using Luna.Publish.Clients;
using Luna.Common.Utils;
using Luna.PubSub.Public.Client;
using Luna.Publish.Public.Client;

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

            builder.Services.TryAddSingleton<IDataMapper<BaseLunaAPIRequest, BaseLunaAPIResponse, BaseLunaAPIProp>, LunaAPIMapper>();
            builder.Services.TryAddSingleton<IDataMapper<LunaApplicationRequest, LunaApplicationResponse, LunaApplicationProp>, LunaApplicationMapper>();

            builder.Services.AddSingleton<IAppEventContentGenerator, AppEventContentGenerator>();
            builder.Services.AddSingleton<IAppEventProcessor, AppEventProcessor>();

            builder.Services.AddSingleton<IHttpRequestParser, HttpRequestParser>();

            string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            
            // Database context must be registered with the dependency injection (DI) container
            builder.Services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.TryAddScoped<ISqlDbContext, SqlDbContext>();

            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
