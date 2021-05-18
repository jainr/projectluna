using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using Luna.PubSub.Utils;

[assembly: FunctionsStartup(typeof(Luna.PubSub.Functions.Startup))]

namespace Luna.PubSub.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<AzureStorageConfiguration>().Configure(
                options =>
                {
                    options.StorageAccountConnectiongString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING");
                });

            builder.Services.AddSingleton<IAzureStorageUtils, AzureStorageUtils>();
            builder.Services.AddSingleton<IEventStoreClient, EventStoreClient>();

            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
