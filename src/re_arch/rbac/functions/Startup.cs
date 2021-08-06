using Luna.RBAC.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Luna.RBAC.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Luna.Common.Utils;
using Luna.RBAC.Public.Client;

[assembly: FunctionsStartup(typeof(Luna.RBAC.Functions.Startup))]

namespace Luna.RBAC.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");

            builder.Services.TryAddSingleton<IDataMapper<RoleAssignmentRequest, RoleAssignmentResponse, RoleAssignmentDb>, RoleAssignmentMapper>();

            builder.Services.TryAddSingleton<IDataMapper<OwnershipRequest, OwnershipResponse, OwnershipDb>, OwnershipMapper>();

            builder.Services.TryAddScoped<IRBACFunctionsImpl, RBACFunctionsImpl>();

            builder.Services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.TryAddScoped<ISqlDbContext, SqlDbContext>();

            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
