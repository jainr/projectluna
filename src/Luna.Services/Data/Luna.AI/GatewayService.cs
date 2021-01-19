using Luna.Clients.Azure;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Azure.Storage;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Enums;
using Luna.Data.Repository;
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luna.Services.Data
{
    public class GatewayService : IGatewayService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<GatewayService> _logger;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private readonly IOptionsMonitor<AzureConfigurationOption> _options;
        private readonly IStorageUtility _storageUtility;

        public GatewayService(IOptionsMonitor<AzureConfigurationOption> options,
            ISqlDbContext sqlDbContext, ILogger<GatewayService> logger, IKeyVaultHelper keyVaultHelper, IStorageUtility storageUtility)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyVaultHelper = keyVaultHelper ?? throw new ArgumentNullException(nameof(keyVaultHelper));
            _storageUtility = storageUtility ?? throw new ArgumentNullException(nameof(storageUtility));
        }

        public async Task<Gateway> CreateAsync(Gateway gateway)
        {
            if (gateway is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Gateway).Name),
                    UserErrorCode.PayloadNotProvided);
            }

            if (await ExistsAsync(gateway.Name))
            {
                throw new LunaConflictUserException(LoggingUtils.ComposeAlreadyExistsErrorMessage(typeof(Gateway).Name,
                        gateway.Name.ToString()));
            }

            _logger.LogInformation(LoggingUtils.ComposeCreateResourceMessage(typeof(Gateway).Name, gateway.Name.ToString()));


            gateway.CreatedTime = DateTime.UtcNow;
            gateway.LastUpdatedTime = gateway.CreatedTime;

            _context.Gateways.Add(gateway);
            await _context._SaveChangesAsync();
            _logger.LogInformation(LoggingUtils.ComposeResourceCreatedMessage(typeof(Gateway).Name, gateway.Name.ToString()));
            return gateway;

        }

        public async Task DeleteAsync(string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeDeleteResourceMessage(typeof(Gateway).Name, name));

            var gateway = await GetAsync(name);

            // Remove the agent from the db
            _context.Gateways.Remove(gateway);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceDeletedMessage(typeof(Gateway).Name, name));

            return;
        }

        public async Task<bool> ExistsAsync(string name)
        {
            _logger.LogInformation(LoggingUtils.ComposeCheckResourceExistsMessage(typeof(Gateway).Name, name));

            var count = await _context.Gateways
                .CountAsync(p => (p.Name == name));

            // More than one instance of an object with the same name exists, this should not happen
            if (count > 1)
            {
                throw new NotSupportedException(LoggingUtils.ComposeFoundDuplicatesErrorMessage(typeof(Gateway).Name, name));

            }
            else if (count == 0)
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Gateway).Name, name, false));
                return false;
            }
            else
            {
                _logger.LogInformation(LoggingUtils.ComposeResourceExistsOrNotMessage(typeof(Gateway).Name, name, true));
                // count = 1
                return true;
            }
        }

        public async Task<List<Gateway>> GetAllAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetAllResourcesMessage(typeof(Gateway).Name));

            // Get all products
            var gateways = await _context.Gateways.ToListAsync();
            _logger.LogInformation(LoggingUtils.ComposeReturnCountMessage(typeof(Gateway).Name, gateways.Count()));

            return gateways;
        }

        public async Task<Gateway> GetAsync(string name)
        {
            if (!await ExistsAsync(name))
            {
                throw new LunaNotFoundUserException(LoggingUtils.ComposeNotFoundErrorMessage(typeof(Gateway).Name,
                    name));
            }
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Gateway).Name, name));

            var gateway = await _context.Gateways.SingleOrDefaultAsync(o => (o.Name == name));

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Gateway).Name,
               name));

            return gateway;
        }

        public async Task<Gateway> GetLeastUsedPublicGatewayAsync()
        {
            _logger.LogInformation("Get the least used public gateway by counting the active subscriptions per gateway.");
            // EF doesn't support outer/left join so we have to find a workaround
            var activeSubscriptions = _context.Subscriptions.Where(s => s.Status == nameof(FulfillmentState.Subscribed));
            var publicGateways = _context.Gateways.Where(g => g.IsPrivate == false);
            var sortedGatewayIdList = publicGateways.
                Join(activeSubscriptions,
                gateway => gateway.Id,
                sub => sub.GatewayId,
                (gateway, sub) => new
                {
                    sub.SubscriptionId,
                    gateway.Id
                }).
                GroupBy(v => v.Id).
                OrderBy(v => v.Count()).
                Select(v => v.Key).ToList();

            long gatewayId = 0;
            if (publicGateways.Count() == sortedGatewayIdList.Count)
            {
                // All public gateways has been used by at least one subscription, return the least used one
                gatewayId = sortedGatewayIdList[0];
            }
            else
            {
                // Otherwise, find the first unused public gateway
                foreach(var gateway in publicGateways)
                {
                    if (!sortedGatewayIdList.Contains(gateway.Id))
                    {
                        gatewayId = gateway.Id;
                        break;
                    }
                }
            }

            _logger.LogInformation($"Returning gateway with id {gatewayId} as the least used public gateway.");

            return await _context.Gateways.FindAsync(gatewayId);
        }

        public async Task<Gateway> UpdateAsync(string name, Gateway gateway)
        {
            if (gateway is null)
            {
                throw new LunaBadRequestUserException(LoggingUtils.ComposePayloadNotProvidedErrorMessage(typeof(Gateway).Name),
                    UserErrorCode.PayloadNotProvided);
            }
            _logger.LogInformation(LoggingUtils.ComposeUpdateResourceMessage(typeof(Gateway).Name, name));

            // The only information can be updated in an AIAgent is the key. We don't need to update the database record
            // TODO: disable the old secret

            var dbGateway = await _context.Gateways.SingleOrDefaultAsync(o => (o.Name == name));

            dbGateway.Copy(gateway);
            dbGateway.LastUpdatedTime = DateTime.UtcNow;
            _context.Gateways.Update(dbGateway);
            await _context._SaveChangesAsync();

            _logger.LogInformation(LoggingUtils.ComposeResourceUpdatedMessage(typeof(Gateway).Name, name));

            return gateway;

        }
    }
}
