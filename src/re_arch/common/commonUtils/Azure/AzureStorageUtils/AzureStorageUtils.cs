using Luna.Common.Utils.Events;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Common.Utils.Azure.AzureStorageUtils
{
    public class AzureStorageUtils : IAzureStorageUtils
    {
        private readonly ILogger<AzureStorageUtils> _logger;
        private readonly CloudTableClient _tableClient;

        [ActivatorUtilitiesConstructor]
        public AzureStorageUtils(IOptionsMonitor<AzureStorageConfiguration> option,
            HttpClient httpClient,
            ILogger<AzureStorageUtils> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(option.CurrentValue.StorageAccountConnectiongString);

            _tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
        }

        /// <summary>
        /// Insert an entity to a table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="entity">The entity</param>
        /// <returns>The table result</returns>
        public async Task<TableResult> InsertTableEntity(string tableName, TableEntity entity)
        {
            CloudTable table = _tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

            // Execute the operation.
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);

            return result;
        }

        /// <summary>
        /// Retrive a table entity
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="eventsAfter">The earliest event sequence id</param>
        /// <returns></returns>
        public async Task<List<LunaBaseEventEntity>> RetrieveSortedTableEntities(string tableName, long eventsAfter)
        {
            CloudTable table = _tableClient.GetTableReference(tableName);
            return table.CreateQuery<LunaBaseEventEntity>().
                Where(x => x.EventSequenceId > eventsAfter).
                ToList<LunaBaseEventEntity>().
                OrderBy(x => x.EventSequenceId).
                ToList<LunaBaseEventEntity>();

        }

        /// <summary>
        /// Get ReadOnly SaS connection string for a table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="validInHours">The valid time in hours</param>
        /// <returns>The SaS connection string</returns>
        public async Task<string> GetReadOnlyTableSaSConnectionString(string tableName, int validInHours = 1)
        {
            CloudTable table = _tableClient.GetTableReference(tableName);
            string connectionString = string.Format("{0}{1}{2}", 
                _tableClient.BaseUri.AbsoluteUri, 
                tableName, 
                table.GetSharedAccessSignature(GetServiceSasTokenPolicy(validInHours)));

            return connectionString;
        }

        private SharedAccessTablePolicy GetServiceSasTokenPolicy(int validInHours)
        {
            return new SharedAccessTablePolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(validInHours),
                Permissions = SharedAccessTablePermissions.Query
            };
        }
    }
}
