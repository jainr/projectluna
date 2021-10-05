using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Luna.RBAC.Public.Client;
using Luna.Publish.Public.Client;
using Luna.PubSub.Public.Client;
using Luna.Gallery.Public.Client;
using Luna.Common.Utils;
using Luna.Partner.Public.Client;
using Luna.Marketplace.Public.Client;

[assembly: FunctionsStartup(typeof(Luna.Gateway.Functions.Startup))]

namespace Luna.Gateway.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<GalleryServiceClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("GALLERY_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("GALLERY_SERVICE_KEY");
                });

            builder.Services.AddSingleton<IGalleryServiceClient, GalleryServiceClient>();

            builder.Services.AddOptions<MarketplaceServiceClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("MARKETPLACE_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("MARKETPLACE_SERVICE_KEY");
                });

            builder.Services.AddSingleton<IMarketplaceServiceClient, MarketplaceServiceClient>();

            builder.Services.AddOptions<RBACClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("RBAC_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("RBAC_SERVICE_KEY");
                });

            builder.Services.AddSingleton<IRBACClient, RBACClient>();

            builder.Services.AddApplicationInsightsTelemetry();

        }
    }
}
