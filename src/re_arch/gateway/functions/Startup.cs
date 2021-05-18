using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Luna.Partner.PublicClient.Clients;
using Luna.RBAC.Public.Client;
using Luna.Publish.PublicClient.Clients;
using Luna.PubSub.PublicClient.Clients;
using Luna.Gallery.Public.Client.Clients;

[assembly: FunctionsStartup(typeof(Luna.Gateway.Functions.Startup))]

namespace Luna.Gateway.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

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

            builder.Services.AddOptions<GalleryServiceClientConfiguration>().Configure(
                options =>
                {
                    options.ServiceBaseUrl = Environment.GetEnvironmentVariable("GALLERY_SERVICE_BASE_URL");
                    options.AuthenticationKey = Environment.GetEnvironmentVariable("GALLERY_SERVICE_KEY");
                });

            builder.Services.AddSingleton<IGalleryServiceClient, GalleryServiceClient>();

            string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");

            builder.Services.AddApplicationInsightsTelemetry();

        }
    }
}
