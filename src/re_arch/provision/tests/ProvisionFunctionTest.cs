using Luna.Common.Test;
using Luna.Common.Utils;
using Luna.Provision.Data;
using Luna.Marketplace.Public.Client;
using Luna.Provision.Clients;
using Luna.PubSub.Public.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.Provision.Test
{
    [TestClass]
    public class ProvisionFunctionTest
    {
        private static readonly string PUBLISH_OFFER_EVENT_CONTENT = @"{
		   ""$type"":""Luna.Marketplace.Public.Client.MarketplaceOffer, Luna.Marketplace.Public.Client"",
		   ""OfferId"":""testoffer"",
		   ""Status"":""Published"",
		   ""Properties"":{
			  ""$type"":""Luna.Marketplace.Public.Client.MarketplaceOfferProp, Luna.Marketplace.Public.Client"",
			  ""DisplayName"":""Test Offer"",
			  ""Description"":""This is a test offer"",
			  ""IsManualActivation"":false
		   },
		   ""Plans"":{
			  ""$type"":""System.Collections.Generic.List`1[[Luna.Marketplace.Public.Client.MarketplacePlan, Luna.Marketplace.Public.Client]], System.Private.CoreLib"",
			  ""$values"":[
				 {
					""$type"":""Luna.Marketplace.Public.Client.MarketplacePlan, Luna.Marketplace.Public.Client"",
					""OfferId"":""testoffer"",
					""PlanId"":""testplan"",
					""Properties"":{
					   ""$type"":""Luna.Marketplace.Public.Client.MarketplacePlanProp, Luna.Marketplace.Public.Client"",
					   ""DisplayName"":""Test plan"",
					   ""Description"":""This is a test plan"",
					   ""Mode"":""PaaS"",
					   ""OnSubscribe"":null,
					   ""OnUpdate"":null,
					   ""OnSuspend"":null,
					   ""OnDelete"":null,
					   ""OnPurge"":null,
					   ""lunaApplicationName"":""myapp""
					},
					""Parameters"":{
					   ""$type"":""System.Collections.Generic.List`1[[Luna.Marketplace.Public.Client.MarketplaceParameter, Luna.Marketplace.Public.Client]], System.Private.CoreLib"",
					   ""$values"":[
						  
					   ]
					}
				 }
			  ]
		   },
		   ""Parameters"":{
			  ""$type"":""System.Collections.Generic.List`1[[Luna.Marketplace.Public.Client.MarketplaceParameter, Luna.Marketplace.Public.Client]], System.Private.CoreLib"",
			  ""$values"":[
				 
			  ]
		   },
		   ""ProvisioningStepsSecretName"":""provisioningstepssecretname""
		}";

		private static readonly string PROVISION_STEPS_CONTENT = @"{
		   ""$type"":""System.Collections.Generic.List`1[[Luna.Marketplace.Public.Client.MarketplaceProvisioningStep, Luna.Marketplace.Public.Client]], System.Private.CoreLib"",
		   ""$values"":[
			  {
				 ""$type"":""Luna.Marketplace.Public.Client.MarketplaceProvisioningStep, Luna.Marketplace.Public.Client"",
				 ""Name"":""scriptstep"",
				 ""Type"":""Script"",
				 ""Properties"":{
					""$type"":""Luna.Marketplace.Public.Client.ScriptProvisioningStepProp, Luna.Marketplace.Public.Client"",
					""ScriptPackageUrl"":""https://packageupdated.zip"",
					""EntryScriptFileName"":""setup.sh"",
					""TimeoutInSeconds"":3600,
					""InputArguments"":{
					   ""$type"":""System.Collections.Generic.List`1[[Luna.Marketplace.Public.Client.ScriptArgument, Luna.Marketplace.Public.Client]], System.Private.CoreLib"",
					   ""$values"":[
						  
					   ]
					},
					""IsSynchronized"":false,
					""Description"":""This is a script step, updated""
				 }
			  }
		   ]
		}";

		private static readonly string CREATE_SUBCRIPTION_EVENT_CONTENT = @"{
		   ""$type"":""Luna.Marketplace.Public.Client.MarketplaceSubscriptionEventContent, Luna.Marketplace.Public.Client"",
		   ""PlanPublishedByEventId"":12345,
		   ""ParametersSecretName"":""parameterssecretname"",
		   ""Id"":""3cd19e47-016d-4f7e-a759-7113620b4f33"",
		   ""PublisherId"":""testpublisher"",
		   ""OfferId"":""testoffer"",
		   ""OwnerId"":""xiwu@microsoft.com"",
		   ""Name"":""testsubname"",
		   ""SaaSSubscriptionStatus"":""PendingFulfillmentStart"",
		   ""PlanId"":""testplan"",
		   ""AllowedCustomerOperations"":null,
		   ""Token"":null,
		   ""InputParameters"":null
		}";

		private static readonly string SUBSCRIPTION_PARAMETERS_CONTENT = @"[
		   {
			  ""Name"":""testparam"",
			  ""Type"":""String"",
			  ""Value"":""testvalue"",
			  ""IsSystemParameter"":false
		   }
		]";

		private LunaRequestHeaders _headers;
        private ILogger<ProvisionFunctionsImpl> _logger;

        private IPubSubServiceClient _pubSubClient;
        private IMarketplaceServiceClient _marketplaceClient;
        private IAzureKeyVaultUtils _keyVaultUtils;
        private ISwaggerClient _swaggerClient;
        private IProvisionStepClientFactory _stepFactory;

        private PublishAzureMarketplaceOfferEventEntity _publishOfferEvent;
		private CreateAzureMarketplaceSubscriptionEventEntity _subscriptionEvent;

        [TestInitialize]
        public void TestInitialize()
        {
            this._headers = new LunaRequestHeaders();

            var mock = new Mock<ILogger<ProvisionFunctionsImpl>>();
            this._logger = mock.Object;


            this._pubSubClient = new MockPubSubServiceClient();
            this._marketplaceClient = new MockMarketplaceServiceClient();
            this._keyVaultUtils = new MockKeyVaultUtils();
            this._swaggerClient = new MockSwaggerClient();

            var stepClientFactoryMock = new Mock<ILogger<ProvisionStepClientFactory>>();
            this._stepFactory = new MockProvisionStepClientFactory(stepClientFactoryMock.Object);

			_keyVaultUtils.SetSecretAsync("provisioningstepssecretname", PROVISION_STEPS_CONTENT);
			this._keyVaultUtils.SetSecretAsync("parameterssecretname", SUBSCRIPTION_PARAMETERS_CONTENT);

			this._publishOfferEvent = new PublishAzureMarketplaceOfferEventEntity("testoffer", PUBLISH_OFFER_EVENT_CONTENT);
			this._publishOfferEvent.EventSequenceId = 12345;
			this._subscriptionEvent = new CreateAzureMarketplaceSubscriptionEventEntity(Guid.Parse("3cd19e47-016d-4f7e-a759-7113620b4f33"), CREATE_SUBCRIPTION_EVENT_CONTENT);

		}

        private IProvisionFunctionsImpl GetProvisionFunctionsImpl(ISqlDbContext context)
        {
            return new ProvisionFunctionsImpl(
                context,
                this._logger,
                this._keyVaultUtils,
                this._pubSubClient,
                this._marketplaceClient,
                this._swaggerClient,
                this._stepFactory);
        }

        [TestMethod]
        public async Task ProcessOfferEventTest()
        {
            var builder = new DbContextOptionsBuilder<SqlDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).
                ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore).Log(InMemoryEventId.TransactionIgnoredWarning));
            var options = builder.Options;
            using (var context = new SqlDbContext(options))
            {
                IProvisionFunctionsImpl function = GetProvisionFunctionsImpl(context);

				await function.ProcessMarketplaceOfferEventAsync(this._publishOfferEvent);
				Assert.AreEqual(1, await context.MarketplacePlans.CountAsync());
            }
        }

		[TestMethod]
		public async Task ProcessSubscriptionEventTest()
		{
			var builder = new DbContextOptionsBuilder<SqlDbContext>();
			builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).
				ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore).Log(InMemoryEventId.TransactionIgnoredWarning));
			var options = builder.Options;
			using (var context = new SqlDbContext(options))
			{
				IProvisionFunctionsImpl function = GetProvisionFunctionsImpl(context);

				await function.ProcessMarketplaceOfferEventAsync(this._publishOfferEvent);
				Assert.AreEqual(1, await context.MarketplacePlans.CountAsync());

				await function.ProcessMarketplaceSubscriptionEventAsync(this._subscriptionEvent);
				Assert.AreEqual(1, await context.MarketplaceSubProvisionJobs.CountAsync());
			}
		}

		[TestMethod]
		public async Task ProcessQueueJobTest()
		{
			var builder = new DbContextOptionsBuilder<SqlDbContext>();
			builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).
				ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore).Log(InMemoryEventId.TransactionIgnoredWarning));
			var options = builder.Options;
			using (var context = new SqlDbContext(options))
			{
				IProvisionFunctionsImpl function = GetProvisionFunctionsImpl(context);

				await function.ProcessMarketplaceOfferEventAsync(this._publishOfferEvent);
				Assert.AreEqual(1, await context.MarketplacePlans.CountAsync());

				await function.ProcessMarketplaceSubscriptionEventAsync(this._subscriptionEvent);
				Assert.AreEqual(1, await context.MarketplaceSubProvisionJobs.CountAsync());

				var job = await context.MarketplaceSubProvisionJobs.SingleOrDefaultAsync(x => x.Status == ProvisionStatus.Queued.ToString());

				Assert.IsNotNull(job);

				await function.ActivateQueuedProvisioningJobAsync(job);
				Assert.AreEqual(0, await context.MarketplaceSubProvisionJobs.CountAsync(x => x.Status == ProvisionStatus.Queued.ToString()));
				Assert.AreEqual(1, await context.MarketplaceSubProvisionJobs.CountAsync(x => x.Status == ProvisionStatus.Running.ToString()));
			}
		}

		[TestMethod]
		public async Task ProcessActiveJobTest()
		{
			var builder = new DbContextOptionsBuilder<SqlDbContext>();
			builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).
				ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore).Log(InMemoryEventId.TransactionIgnoredWarning));
			var options = builder.Options;
			using (var context = new SqlDbContext(options))
			{
				IProvisionFunctionsImpl function = GetProvisionFunctionsImpl(context);

				await function.ProcessMarketplaceOfferEventAsync(this._publishOfferEvent);
				Assert.AreEqual(1, await context.MarketplacePlans.CountAsync());

				await function.ProcessMarketplaceSubscriptionEventAsync(this._subscriptionEvent);
				Assert.AreEqual(1, await context.MarketplaceSubProvisionJobs.CountAsync());

				var job = await context.MarketplaceSubProvisionJobs.SingleOrDefaultAsync(x => x.Status == ProvisionStatus.Queued.ToString());

				Assert.IsNotNull(job);

				await function.ActivateQueuedProvisioningJobAsync(job);
				Assert.AreEqual(0, await context.MarketplaceSubProvisionJobs.CountAsync(x => x.Status == ProvisionStatus.Queued.ToString()));
				Assert.AreEqual(1, await context.MarketplaceSubProvisionJobs.CountAsync(x => x.Status == ProvisionStatus.Running.ToString()));

				job = await context.MarketplaceSubProvisionJobs.SingleOrDefaultAsync(x => x.Status == ProvisionStatus.Running.ToString());

				await function.ProcessActiveProvisioningJobStepAsync(job);
				Assert.AreEqual(0, await context.MarketplaceSubProvisionJobs.CountAsync(x => x.Status == ProvisionStatus.Running.ToString()));
				Assert.AreEqual(1, await context.MarketplaceSubProvisionJobs.CountAsync(x => x.Status == ProvisionStatus.Completed.ToString()));

			}
		}

	}
}
