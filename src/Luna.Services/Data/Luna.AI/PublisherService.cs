using Luna.Clients.Azure;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Azure.Storage;
using Luna.Clients.Exceptions;
using Luna.Clients.Logging;
using Luna.Data.Entities;
using Luna.Data.Entities.Luna.AI;
using Luna.Data.Repository;
using Luna.Services.Utilities.ExpressionEvaluation;
using Microsoft.EntityFrameworkCore;
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
    public class PublisherService : IPublisherService
    {
        private readonly ISqlDbContext _context;
        private readonly ILogger<PublisherService> _logger;

        public PublisherService(ISqlDbContext sqlDbContext, ILogger<PublisherService> logger)
        {
            _context = sqlDbContext ?? throw new ArgumentNullException(nameof(sqlDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Publisher> GetAsync()
        {
            _logger.LogInformation(LoggingUtils.ComposeGetSingleResourceMessage(typeof(Publisher).Name, "publisher"));

            var publisher = await _context.Publishers.SingleOrDefaultAsync();

            _logger.LogInformation(LoggingUtils.ComposeReturnValueMessage(typeof(Publisher).Name,
               publisher.PublisherId.ToString()));

            return publisher;
        }
    }
}
