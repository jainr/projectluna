using Luna.Common.Utils;
using Luna.Marketplace.Public.Client;
using Luna.Provision.Data;
using Luna.PubSub.Public.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Provision.Clients
{
    public class ProvisionFunctionsImpl : IProvisionFunctionsImpl
    {
        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<ProvisionFunctionsImpl> _logger;
        private readonly IPubSubServiceClient _pubSubClient;
        private readonly IAzureKeyVaultUtils _keyVaultUtils;
        private readonly IMarketplaceServiceClient _marketplaceClient;
        private readonly ISwaggerClient _swaggerClient;
        private readonly IProvisionStepClientFactory _provisionStepClientFactory;

        public ProvisionFunctionsImpl(ISqlDbContext dbContext,
            ILogger<ProvisionFunctionsImpl> logger,
            IAzureKeyVaultUtils keyVaultUtils,
            IPubSubServiceClient pubSubClient,
            IMarketplaceServiceClient marketplaceClient,
            ISwaggerClient swaggerClient,
            IProvisionStepClientFactory provisionStepClientFactory)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(dbContext));
            this._pubSubClient = pubSubClient ?? throw new ArgumentNullException(nameof(pubSubClient));
            this._marketplaceClient = marketplaceClient ?? throw new ArgumentNullException(nameof(marketplaceClient));
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
            this._swaggerClient = swaggerClient ?? throw new ArgumentNullException(nameof(swaggerClient));
            this._provisionStepClientFactory = provisionStepClientFactory ?? throw new ArgumentNullException(nameof(provisionStepClientFactory));
        }

        public async Task ProcessActiveProvisioningJobStepAsync(MarketplaceSubProvisionJobDB job)
        {
            try
            {
                var parameters = await GetParametersAsync(job);
                var stepConfigs = await GetProvisionStepConfigAsync(job);
                var steps = await GetProvisionStepsAsync(job);

                if (job.ProvisioningStepIndex >= steps.Count)
                {
                    var e = new LunaServerException("The provisioning step index is invalid");
                }
                else
                {
                    MarketplaceProvisioningStep stepConfig = null;

                    if (job.ProvisioningStepIndex < 0)
                    {
                        if (job.Mode.Equals(MarketplacePlanMode.SaaS.ToString()) && job.LunaApplicationName != null)
                        {
                            var url = Environment.GetEnvironmentVariable("GALLERY_SERVICE_BASE_URL");
                            stepConfig = new MarketplaceProvisioningStep
                            {
                                Name = "SubscribeLunaApplication",
                                Type = MarketplaceProvisioningStepType.Webhook.ToString(),
                                Properties = new WebhookProvisioningStepProp
                                {
                                    WebhookUrl = url != null ? $"{url}subscriptions/create" : null,
                                    WebhookAuthType = WebhookAuthType.ApiKey.ToString(),
                                    WebhookAuthKey = "x-functions-key",
                                    WebhookAuthValue = Environment.GetEnvironmentVariable("GALLERY_SERVICE_KEY"),
                                    InputParameterNames = new List<string>(new string[] { "SubscriptionId", "SubscriptionName", "OwnerId", "LunaApplicationName" }),
                                    OutputParameterNames = new List<string>(new string[] { "BaseUrl", "PrimaryKey", "SecondaryKey" }),
                                }
                            };
                        }
                        else if (job.Mode.Equals(MarketplacePlanMode.IaaS.ToString()) && !IsJumpboxReady(parameters))
                        {
                            // Generate new SSH key pair if not exist
                            if (!parameters.Any(x => x.Name == JumpboxParameterConstants.JUMPBOX_VM_SSH_PUBLIC_KEY_PARAM_NAME))
                            {
                                SSHKeyPair keyPair = SshUtils.GetSSHKeyPair();

                                parameters.Add(new MarketplaceSubscriptionParameter
                                {
                                    Name = JumpboxParameterConstants.JUMPBOX_VM_SSH_PUBLIC_KEY_PARAM_NAME,
                                    Value = keyPair.PublicKey,
                                    Type = MarketplaceParameterValueType.String.ToString(),
                                    IsSystemParameter = true
                                });

                                parameters.Add(new MarketplaceSubscriptionParameter
                                {
                                    Name = JumpboxParameterConstants.JUMPBOX_VM_SSH_PRIVATE_KEY_PARAM_NAME,
                                    Value = keyPair.PrivateKey,
                                    Type = MarketplaceParameterValueType.String.ToString(),
                                    IsSystemParameter = true
                                });
                            }

                            stepConfig = new MarketplaceProvisioningStep()
                            {
                                Name = "JumpboxProvisioning",
                                Type = MarketplaceProvisioningStepType.ARMTemplate.ToString(),
                                Properties = new ARMTemplateProvisioningStepProp
                                {
                                    TemplateUrl = Environment.GetEnvironmentVariable("DEPLOY_JB_ARM_TEMPLATE"),
                                    IsRunInCompleteMode = false,
                                    AzureSubscriptionIdParameterName = JumpboxParameterConstants.JUMPBOX_VM_SUB_ID_PARAM_NAME,
                                    ResourceGroupNameParameterName = JumpboxParameterConstants.JUMPBOX_VM_RG_PARAM_NAME,
                                    AccessTokenParameterName = JumpboxParameterConstants.JUMPBOX_VM_ACCESS_TOKEN_PARAM_NAME,
                                    AzureLocationParameterName = JumpboxParameterConstants.JUMPBOX_VM_LOCATION_PARAM_NAME,
                                    InputParameterNames = new List<string>
                                        {
                                            JumpboxParameterConstants.JUMPBOX_VM_LOCATION_PARAM_NAME,
                                            JumpboxParameterConstants.JUMPBOX_VM_NAME_PARAM_NAME,
                                            JumpboxParameterConstants.JUMPBOX_VM_SSH_PUBLIC_KEY_PARAM_NAME,
                                        }
                                }
                            };
                        }
                        else
                        {
                            // skip the jump box preparation step if it is not required
                            job.ProvisioningStepIndex = 0;
                            job.ProvisioningStepStatus = ProvisionStepStatus.NotStarted.ToString();
                            job.LastUpdatedTime = DateTime.UtcNow;
                            _dbContext.MarketplaceSubProvisionJobs.Update(job);
                            await _dbContext._SaveChangesAsync();
                            return;
                        }

                    }
                    else
                    {
                        var step = steps[job.ProvisioningStepIndex];

                        stepConfig = stepConfigs.SingleOrDefault(x => x.Name == step);
                    }

                    if (stepConfig == null)
                    {
                        throw new LunaServerException("");
                    }

                    if (stepConfig.Properties.IsSynchronized)
                    {
                        ISyncProvisionStepClient client = this._provisionStepClientFactory.GetSyncProvisionStepClient(stepConfig);
                        var newParams = await client.RunAsync(parameters);

                        var content = JsonConvert.SerializeObject(newParams, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.All
                        });

                        await this._keyVaultUtils.SetSecretAsync(job.ParametersSecretName, content);

                        if (job.ProvisioningStepIndex + 1 < steps.Count)
                        {
                            job.ProvisioningStepIndex = job.ProvisioningStepIndex + 1;
                            job.ProvisioningStepStatus = ProvisionStepStatus.NotStarted.ToString();
                        }
                        else
                        {
                            await this._marketplaceClient.ActivateMarketplaceSubscriptionAsync(job.SubscriptionId, new LunaRequestHeaders());
                            job.Status = ProvisionStatus.Completed.ToString();
                            job.IsActive = false;
                            job.CompletedTime = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        IAsyncProvisionStepClient client = this._provisionStepClientFactory.GetAsyncProvisionStepClient(stepConfig);

                        if (client != null)
                        {
                            if (job.ProvisioningStepStatus.Equals(ProvisionStepStatus.NotStarted.ToString()))
                            {
                                // TODO: should copy over
                                var newParams = await client.StartAsync(parameters);

                                var content = JsonConvert.SerializeObject(newParams, new JsonSerializerSettings()
                                {
                                    TypeNameHandling = TypeNameHandling.All
                                });

                                await this._keyVaultUtils.SetSecretAsync(job.ParametersSecretName, content);

                                job.ProvisioningStepStatus = ProvisionStepStatus.Running.ToString();

                            }
                            else if (job.ProvisioningStepStatus.Equals(ProvisionStepStatus.Running.ToString()))
                            {
                                var result = await client.CheckExecutionStatusAsync(parameters);
                                switch (result)
                                {
                                    case ProvisionStepExecutionResult.Completed:
                                        job.ProvisioningStepStatus = ProvisionStepStatus.ExecutionCompleted.ToString();
                                        break;
                                    case ProvisionStepExecutionResult.Running:
                                        break;
                                    case ProvisionStepExecutionResult.Failed:
                                        job.ProvisioningStepStatus = ProvisionStepStatus.Failed.ToString();
                                        break;
                                    default:
                                        throw new LunaServerException($"invalid provision step result {result.ToString()}");
                                }
                            }
                            else if (job.ProvisioningStepStatus.Equals(ProvisionStepStatus.ExecutionCompleted.ToString()))
                            {
                                var newParams = await client.FinishAsync(parameters);

                                var content = JsonConvert.SerializeObject(newParams, new JsonSerializerSettings()
                                {
                                    TypeNameHandling = TypeNameHandling.All
                                });

                                await this._keyVaultUtils.SetSecretAsync(job.ParametersSecretName, content);

                                job.ProvisioningStepStatus = ProvisionStepStatus.JobCompleted.ToString();
                            }
                            else if (job.ProvisioningStepStatus.Equals(ProvisionStepStatus.JobCompleted.ToString()))
                            {
                                if (job.ProvisioningStepIndex + 1 < steps.Count)
                                {
                                    job.ProvisioningStepIndex = job.ProvisioningStepIndex + 1;
                                    job.ProvisioningStepStatus = ProvisionStepStatus.NotStarted.ToString();
                                }
                                else
                                {
                                    await this._marketplaceClient.ActivateMarketplaceSubscriptionAsync(job.SubscriptionId, new LunaRequestHeaders());
                                    job.Status = ProvisionStatus.Completed.ToString();
                                    job.IsActive = false;
                                    job.CompletedTime = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                }

                job.LastUpdatedTime = DateTime.UtcNow;
                _dbContext.MarketplaceSubProvisionJobs.Update(job);
                await _dbContext._SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                job.LastErrorMessage = e.Message;
                job.LastUpdatedTime = DateTime.UtcNow;
                _dbContext.MarketplaceSubProvisionJobs.Update(job);
                await _dbContext._SaveChangesAsync();
            }
        }

        public async Task<Guid?> ActivateQueuedProvisioningJobAsync(MarketplaceSubProvisionJobDB job)
        {
            // make it active
            var plan = await _dbContext.MarketplacePlans.
                SingleOrDefaultAsync(x => x.OfferId == job.OfferId &&
                    x.PlanId == job.PlanId &&
                    x.CreatedByEventId == job.PlanCreatedByEventId);

            if (plan == null)
            {
                var error = $"Can not find marketplace plan {job.PlanId} in offer {job.OfferId}" +
                    $" created by event {job.PlanCreatedByEventId} for subscription {job.SubscriptionId}";
                _logger.LogError(error);
                job.Status = ProvisionStatus.Aborted.ToString();
                job.LastErrorMessage = error;
                return null;
            }
            else
            {
                job.Status = ProvisionStatus.Running.ToString();
                job.IsActive = true;
            }

            job.LastUpdatedTime = DateTime.UtcNow;
            _dbContext.MarketplaceSubProvisionJobs.Update(job);
            await _dbContext._SaveChangesAsync();

            return job.SubscriptionId;
        }

        public async Task ProcessMarketplaceOfferEventAsync(LunaBaseEventEntity ev)
        {
            if (ev.EventType.Equals(LunaEventType.PUBLISH_AZURE_MARKETPLACE_OFFER))
            {
                MarketplaceOffer offer = JsonConvert.DeserializeObject<MarketplaceOffer>(ev.EventContent, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

                foreach (var plan in offer.Plans)
                {
                    var planDb = new MarketplacePlanDB();
                    planDb.OfferId = offer.OfferId;
                    planDb.PlanId = plan.PlanId;
                    planDb.LunaApplicationName = plan.Properties.LunaApplicationName;
                    planDb.Mode = plan.Properties.Mode;
                    planDb.Properties = JsonConvert.SerializeObject(plan.Properties, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                    var parameters = new List<MarketplaceParameter>();
                    parameters.AddRange(offer.Parameters);
                    parameters.AddRange(plan.Parameters);
                    planDb.Parameters = JsonConvert.SerializeObject(parameters, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                    planDb.CreatedByEventId = ev.EventSequenceId;
                    planDb.ProvisioningStepsSecretName = offer.ProvisioningStepsSecretName;

                    _dbContext.MarketplacePlans.Add(planDb);
                }

                await _dbContext._SaveChangesAsync();
            }
            else
            {
                throw new LunaServerException($"Unknown event type {ev.EventType}.");
            }
        }

        public async Task ProcessMarketplaceSubscriptionEventAsync(LunaBaseEventEntity ev)
        {
            if (ev.EventType.Equals(LunaEventType.CREATE_AZURE_MARKETPLACE_SUBSCRIPTION))
            {
                var sub = JsonConvert.DeserializeObject<MarketplaceSubscriptionEventContent>(ev.EventContent);

                var plan = await _dbContext.MarketplacePlans.
                    SingleOrDefaultAsync(x => x.OfferId == sub.OfferId &&
                    x.PlanId == sub.PlanId && x.CreatedByEventId == sub.PlanPublishedByEventId);

                if (plan == null)
                {
                    throw new LunaServerException($"Plan {sub.PlanId} in offer {sub.OfferId} created by event {sub.PlanPublishedByEventId} does not exist.");
                }

                var currentTime = DateTime.UtcNow;

                var content = await this._keyVaultUtils.GetSecretAsync(sub.ParametersSecretName);
                var parameters = JsonConvert.DeserializeObject<List<MarketplaceSubscriptionParameter>>(content, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

                parameters.Add(new MarketplaceSubscriptionParameter
                {
                    Name = "SubscriptionId",
                    IsSystemParameter = true,
                    Value = sub.Id.ToString(),
                    Type = MarketplaceParameterValueType.String.ToString(),
                });

                parameters.Add(new MarketplaceSubscriptionParameter
                {
                    Name = "SubscriptionName",
                    IsSystemParameter = true,
                    Value = sub.Name,
                    Type = MarketplaceParameterValueType.String.ToString(),
                });

                parameters.Add(new MarketplaceSubscriptionParameter
                {
                    Name = "OwnerId",
                    IsSystemParameter = true,
                    Value = sub.OwnerId,
                    Type = MarketplaceParameterValueType.String.ToString(),
                });

                if (plan.LunaApplicationName != null)
                {
                    parameters.Add(new MarketplaceSubscriptionParameter
                    {
                        Name = "LunaApplicationName",
                        IsSystemParameter = true,
                        Value = plan.LunaApplicationName,
                        Type = MarketplaceParameterValueType.String.ToString(),
                    });
                }

                await this._keyVaultUtils.SetSecretAsync(sub.ParametersSecretName,
                    JsonConvert.SerializeObject(parameters, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    }));

                var jobDb = new MarketplaceSubProvisionJobDB()
                {
                    SubscriptionId = sub.Id,
                    OfferId = sub.OfferId,
                    PlanId = sub.PlanId,
                    PlanCreatedByEventId = sub.PlanPublishedByEventId,
                    Mode = plan.Mode,
                    Status = ProvisionStatus.Queued.ToString(),
                    EventType = ev.EventType,
                    LunaApplicationName = plan.LunaApplicationName,
                    ProvisioningStepIndex = -1,
                    IsSynchronizedStep = false,
                    ProvisioningStepStatus = ProvisionStepStatus.NotStarted.ToString(),
                    ParametersSecretName = sub.ParametersSecretName,
                    ProvisionStepsSecretName = plan.ProvisioningStepsSecretName,
                    IsActive = false,
                    RetryCount = 0,
                    CreatedByEventId = ev.EventSequenceId,
                    CreatedTime = currentTime,
                    LastUpdatedTime = currentTime,
                };

                using (var transaction = await _dbContext.BeginTransactionAsync())
                {
                    // Can have only one create job for a subscription
                    if (!await _dbContext.MarketplaceSubProvisionJobs.
                        AnyAsync(x => x.SubscriptionId == sub.Id && x.EventType == ev.EventType))
                    {
                        this._dbContext.MarketplaceSubProvisionJobs.Add(jobDb);
                        await this._dbContext._SaveChangesAsync();
                    }

                    transaction.Commit();
                }
            }
            else
            {
                throw new LunaServerException($"Unknown event type {ev.EventType}.");
            }
        }

        #region private methods

        private async Task<List<MarketplaceSubscriptionParameter>> GetParametersAsync(MarketplaceSubProvisionJobDB job)
        {
            var content = await this._keyVaultUtils.GetSecretAsync(job.ParametersSecretName);
            var parameters = JsonConvert.DeserializeObject<List<MarketplaceSubscriptionParameter>>(content, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            return parameters;
        }

        private async Task<List<MarketplaceProvisioningStep>> GetProvisionStepConfigAsync(MarketplaceSubProvisionJobDB job)
        {
            var content = await this._keyVaultUtils.GetSecretAsync(job.ProvisionStepsSecretName);
            var steps = JsonConvert.DeserializeObject<List<MarketplaceProvisioningStep>>(content, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return steps;
        }

        private async Task<List<string>> GetProvisionStepsAsync(MarketplaceSubProvisionJobDB job)
        {
            var plan = await _dbContext.MarketplacePlans.
                SingleOrDefaultAsync(x => x.OfferId == job.OfferId && x.PlanId == job.PlanId && x.CreatedByEventId == job.PlanCreatedByEventId);

            var prop = JsonConvert.DeserializeObject<MarketplacePlanProp>(plan.Properties);
            if (job.EventType.Equals(LunaEventType.CREATE_AZURE_MARKETPLACE_SUBSCRIPTION))
            {
                return prop.OnSubscribe == null ? new List<string>() : prop.OnSubscribe;
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        private bool IsJumpboxReady(List<MarketplaceSubscriptionParameter> parameters)
        {
            return JumpboxParameterConstants.HasConnectionInfo(parameters.Select(x => x.Name).ToList());
        }
        #endregion
    }
}
