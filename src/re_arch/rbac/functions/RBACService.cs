using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Luna.RBAC.Data.DataContracts;
using Luna.RBAC.Clients;
using Luna.RBAC.Data.Entities;

namespace Luna.RBAC
{
    /// <summary>
    /// The service maintains all RBAC rules
    /// </summary>
    public class RBACService
    {
        private readonly IRBACCacheClient _cacheClient;
        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<RBACService> _logger;

        public RBACService(IRBACCacheClient cacheClient, ISqlDbContext dbContext, ILogger<RBACService> logger)
        {
            this._cacheClient = cacheClient;
            this._dbContext = dbContext;
            this._logger = logger;
        }

        /// <summary>
        /// Add a RBAC rule
        /// </summary>
        /// <param name="req">The http request</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [FunctionName("AddRBACRule")]
        public async Task<IActionResult> AddRBACRule(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "rbacrule")] HttpRequest req)
        {
            this._logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            RBACRule rule = (RBACRule)JsonConvert.DeserializeObject(requestBody, typeof(RBACRule));
            rule.CreatedTime = DateTime.UtcNow;
            rule.LastUpdatedTime = rule.CreatedTime;
            _dbContext.RBACRules.Add(rule);
            await _dbContext._SaveChangesAsync();

            _cacheClient.AddRBACRuleToCache(rule);

            return new OkObjectResult(rule);
        }
    }
}
