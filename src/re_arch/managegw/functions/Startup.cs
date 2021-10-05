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
            builder.Services.AddOptions<EncryptionConfiguration>().Configure(
                options =>
                {
                    options.SymmetricKey = Environment.GetEnvironmentVariable("ENCRYPTION_ASYMMETRIC_KEY");
                });

            builder.Services.AddSingleton<IEncryptionUtils, EncryptionUtils>();

            builder.Services.AddOptions<PartnerServiceClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("PARTNER_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("PARTNER_SERVICE_KEY");
                });
            
            builder.Services.AddSingleton<IPartnerServiceClient, PartnerServiceClient>();


            builder.Services.AddOptions<RBACClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("RBAC_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("RBAC_SERVICE_KEY");
                });

            builder.Services.AddSingleton<IRBACClient, RBACClient>();

            builder.Services.AddOptions<PublishServiceClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("PUBLISH_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("PUBLISH_SERVICE_KEY");
                });

            builder.Services.AddSingleton<IPublishServiceClient, PublishServiceClient>();

            builder.Services.AddOptions<PubSubServiceClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("PUBSUB_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("PUBSUB_SERVICE_KEY");
                });

            builder.Services.AddSingleton<IPubSubServiceClient, PubSubServiceClient>();

            builder.Services.AddOptions<MarketplaceServiceClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("MARKETPLACE_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("MARKETPLACE_SERVICE_KEY");
                });

            builder.Services.AddSingleton<IMarketplaceServiceClient, MarketplaceServiceClient>();

            builder.Services.AddApplicationInsightsTelemetry();

        }
    }
}
