using Luna.Common.Utils;
using Luna.Marketplace.Clients;
using Luna.Marketplace.Data;
using Luna.Marketplace.Public.Client;
using Luna.Marketplace.Test.Mock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.Marketplace.Test
{
    [TestClass]
    public class MarketplaceFunctionTest
    {
        private LunaRequestHeaders _headers;
        private ILogger<MarketplaceFunctionsImpl> _logger;
        private ILogger<OfferEventContentGenerator> _contentGeneratorLogger;

        private MarketplaceOfferRequest _offerRequest;
        private MarketplaceOfferRequest _updatedOfferRequest;
        private string _userId;

        private MarketplacePlanRequest _planRequest;
        private MarketplacePlanRequest _updatedPlanRequest;
        private MarketplacePlanRequest _secondPlanRequest;

        private MarketplaceParameterRequest _paramRequest;
        private MarketplaceParameterRequest _updatedParamRequest;
        private MarketplaceParameterRequest _secondParamRequest;

        private ScriptProvisioningStepRequest _scriptStepRequest;
        private ScriptProvisioningStepRequest _updatedScriptStepRequest;
        private ARMTemplateProvisioningStepRequest _armStepRequest;
        private WebhookProvisioningStepRequest _webhookStepRequest;

        [TestInitialize]
        public void TestInitialize()
        {
            this._headers = new LunaRequestHeaders();

            var mock = new Mock<ILogger<MarketplaceFunctionsImpl>>();
            this._logger = mock.Object;

            var contentGeneratorMock = new Mock<ILogger<OfferEventContentGenerator>>();
            this._contentGeneratorLogger = contentGeneratorMock.Object;

            this._userId = "testuser";
            this._headers.UserId = this._userId;

            this._offerRequest = new MarketplaceOfferRequest
            {
                OfferId = "testoffer",
                DisplayName = "Test Offer",
                Description = "This is a test offer",

                IsManualActivation = false,
            };

            this._updatedOfferRequest = new MarketplaceOfferRequest
            {
                OfferId = "testoffer",
                DisplayName = "Updated Test Offer",
                Description = "This is a test offer, updated",

                IsManualActivation = false,
            };

            this._planRequest = new MarketplacePlanRequest
            {
                PlanId = "testplan",
                DisplayName = "Test plan",
                Description = "This is a test plan",
                Mode = MarketplacePlanMode.IaaS.ToString(),
                LunaApplicationName = "myapp",
            };

            this._updatedPlanRequest = new MarketplacePlanRequest
            {
                PlanId = "testplan",
                DisplayName = "Test plan, updated",
                Description = "This is a test plan, updated",
                Mode = MarketplacePlanMode.IaaS.ToString(),
                LunaApplicationName = "myappupdated",
            };

            this._secondPlanRequest = new MarketplacePlanRequest
            {
                PlanId = "testplan2",
                DisplayName = "Test plan 2",
                Description = "This is another test plan",
                Mode = MarketplacePlanMode.PaaS.ToString(),
                LunaApplicationName = "myapp2",
            };

            this._paramRequest = new MarketplaceParameterRequest
            {
                ParameterName = "testparam",
                DisplayName = "Test Param",
                Description = "This is a test parameter",
                ValueType = MarketplaceParameterValueType.String.ToString(),
                FromList = true,
                ValueList = "value1;value2",
                IsRequired = true,
                IsUserInput = true,
                DefaultValue = "value1"
            };

            this._updatedParamRequest = new MarketplaceParameterRequest
            {
                ParameterName = "testparam",
                DisplayName = "Test Param Updated",
                Description = "This is a test parameter, updated",
                ValueType = MarketplaceParameterValueType.String.ToString(),
                FromList = true,
                ValueList = "value1;value2;value3",
                IsRequired = false,
                IsUserInput = true
            };

            this._secondParamRequest = new MarketplaceParameterRequest
            {
                ParameterName = "testparam2",
                DisplayName = "Test Param 2",
                Description = "This is another test parameter",
                ValueType = MarketplaceParameterValueType.Number.ToString(),
                FromList = false,
                Maximum = 100,
                Minimum = 10,
                IsRequired = true,
                IsUserInput = true,
                DefaultValue = 33
            };

            this._scriptStepRequest = new ScriptProvisioningStepRequest
            {
                Name = "scriptstep",
                Type = MarketplaceProvisioningStepType.Script.ToString(),
                Description = "This is a script step",
                ScriptPackageUrl = "https://package.zip",
                EntryScriptFileName = "setup.sh",
                TimeoutInSeconds = 3600,
                InputArguments = new List<ScriptArgumentRequest>()
            };

            this._scriptStepRequest.InputArguments.Add(new ScriptArgumentRequest
            {
                Name = "argname",
                Option = "n",
            });

            this._updatedScriptStepRequest = new ScriptProvisioningStepRequest
            {
                Name = "scriptstep",
                Type = MarketplaceProvisioningStepType.Script.ToString(),
                Description = "This is a script step, updated",
                ScriptPackageUrl = "https://packageupdated.zip",
                EntryScriptFileName = "setup.sh",
                TimeoutInSeconds = 3600,
                InputArguments = new List<ScriptArgumentRequest>()
            };

            this._armStepRequest = new ARMTemplateProvisioningStepRequest
            {
                Name = "armstep",
                Type = MarketplaceProvisioningStepType.ARMTemplate.ToString(),
                Description = "This is an ARM step.",
                TemplateUrl = "https://template.json",
                IsRunInCompleteMode = true,
                AzureSubscriptionIdParameterName = "subscriptionId",
                AzureLocationParameterName = "location",
                ResourceGroupNameParameterName = "resourceGroup",
                AccessTokenParameterName = "accessToken",
                InputParameterNames = new List<string>(new string[] { "parameter" }),
            };

            this._webhookStepRequest = new WebhookProvisioningStepRequest
            {
                Name = "webhookstep",
                Type = MarketplaceProvisioningStepType.Webhook.ToString(),
                Description = "This is a webhook step.",
                WebhookUrl = "https://webhook.com",
                WebhookAuthType = WebhookAuthType.ApiKey.ToString(),
                WebhookAuthKey = "testkey",
                WebhookAuthValue = "testvalue",
                TimeoutInSeconds = 3600,
                InputParameterNames = new List<string>(new string[] { "parameter" }),
            };

        }

        [TestMethod]
        public async Task BasicOfferTest()
        {
            var builder = new DbContextOptionsBuilder<SqlDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).
                ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore).Log(InMemoryEventId.TransactionIgnoredWarning));
            var options = builder.Options;
            using (var context = new SqlDbContext(options))
            {
                IMarketplaceFunctionsImpl function = new MarketplaceFunctionsImpl(
                    new OfferEventProcessor(),
                    new OfferEventContentGenerator(new MockKeyVaultUtils(), this._contentGeneratorLogger),
                    new MarketplaceOfferMapper(),
                    new MarketplacePlanMapper(),
                    new MarketplaceParameterMapper(),
                    new MarketplaceProvisioningStepMapper(),
                    context,
                    this._logger);

                var offerResponse = await function.CreateMarketplaceOfferAsync(this._offerRequest.OfferId, this._offerRequest, this._headers);
                Assert.IsInstanceOfType(offerResponse, typeof(MarketplaceOfferResponse));
                Assert.AreEqual(offerResponse.DisplayName, this._offerRequest.DisplayName);
                var offers = await function.ListMarketplaceOffersAsync(this._userId, this._headers);
                Assert.AreEqual(1, offers.Count);

                await Assert.ThrowsExceptionAsync<LunaConflictUserException>(
                    () => function.CreateMarketplaceOfferAsync(this._offerRequest.OfferId, this._offerRequest, this._headers));

                offerResponse = await function.UpdateMarketplaceOfferAsync(this._updatedOfferRequest.OfferId, this._updatedOfferRequest, this._headers);
                Assert.IsInstanceOfType(offerResponse, typeof(MarketplaceOfferResponse));
                Assert.AreEqual(offerResponse.DisplayName, this._updatedOfferRequest.DisplayName);

                var offer = await function.GetMarketplaceOfferAsync(this._offerRequest.OfferId, this._headers);
                Assert.IsInstanceOfType(offerResponse, typeof(MarketplaceOfferResponse));
                Assert.AreEqual(offerResponse.DisplayName, this._updatedOfferRequest.DisplayName);
                Assert.AreEqual(MarketplaceOfferStatus.Draft.ToString(), offer.Status);

                await function.PublishMarketplaceOfferAsync(this._offerRequest.OfferId, this._headers); 

                offer = await function.GetMarketplaceOfferAsync(this._offerRequest.OfferId, this._headers);
                Assert.IsInstanceOfType(offerResponse, typeof(MarketplaceOfferResponse));
                Assert.AreEqual(offerResponse.DisplayName, this._updatedOfferRequest.DisplayName);
                Assert.AreEqual(MarketplaceOfferStatus.Published.ToString(), offer.Status);

                await function.DeleteMarketplaceOfferAsync(this._offerRequest.OfferId, this._headers);
                await Assert.ThrowsExceptionAsync<LunaNotFoundUserException>(
                    () => function.GetMarketplaceOfferAsync(this._offerRequest.OfferId, this._headers));

                offers = await function.ListMarketplaceOffersAsync(this._userId, this._headers);
                Assert.AreEqual(0, offers.Count);

            }
        }


        [TestMethod]
        public async Task BasicPlanTest()
        {
            var builder = new DbContextOptionsBuilder<SqlDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).
                ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore).Log(InMemoryEventId.TransactionIgnoredWarning));
            var options = builder.Options;
            using (var context = new SqlDbContext(options))
            {
                IMarketplaceFunctionsImpl function = new MarketplaceFunctionsImpl(
                    new OfferEventProcessor(),
                    new OfferEventContentGenerator(new MockKeyVaultUtils(), this._contentGeneratorLogger),
                    new MarketplaceOfferMapper(),
                    new MarketplacePlanMapper(),
                    new MarketplaceParameterMapper(),
                    new MarketplaceProvisioningStepMapper(),
                    context,
                    this._logger);

                var offerResponse = await function.CreateMarketplaceOfferAsync(this._offerRequest.OfferId, this._offerRequest, this._headers);
                Assert.IsInstanceOfType(offerResponse, typeof(MarketplaceOfferResponse));
                Assert.AreEqual(offerResponse.DisplayName, this._offerRequest.DisplayName);

                var planResponse = await function.CreateMarketplacePlanAsync(offerResponse.OfferId, this._planRequest.PlanId, this._planRequest, this._headers);
                Assert.IsInstanceOfType(planResponse, typeof(MarketplacePlanResponse));
                Assert.AreEqual(planResponse.DisplayName, this._planRequest.DisplayName);

                planResponse = await function.UpdateMarketplacePlanAsync(offerResponse.OfferId, this._updatedPlanRequest.PlanId, this._updatedPlanRequest, this._headers);
                Assert.IsInstanceOfType(planResponse, typeof(MarketplacePlanResponse));
                Assert.AreEqual(planResponse.DisplayName, this._updatedPlanRequest.DisplayName);

                planResponse = await function.CreateMarketplacePlanAsync(offerResponse.OfferId, this._secondPlanRequest.PlanId, this._secondPlanRequest, this._headers);
                Assert.IsInstanceOfType(planResponse, typeof(MarketplacePlanResponse));
                Assert.AreEqual(planResponse.DisplayName, this._secondPlanRequest.DisplayName);

                var plans = await function.ListMarketplacePlansAsync(offerResponse.OfferId, this._headers);
                Assert.AreEqual(2, plans.Count);

                await function.DeleteMarketplacePlanAsync(offerResponse.OfferId, this._planRequest.PlanId, this._headers);

                plans = await function.ListMarketplacePlansAsync(offerResponse.OfferId, this._headers);
                Assert.AreEqual(1, plans.Count);
            }
        }

        [TestMethod]
        public async Task BasicOfferParameterTest()
        {
            var builder = new DbContextOptionsBuilder<SqlDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).
                ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore).Log(InMemoryEventId.TransactionIgnoredWarning));
            var options = builder.Options;
            using (var context = new SqlDbContext(options))
            {
                IMarketplaceFunctionsImpl function = new MarketplaceFunctionsImpl(
                    new OfferEventProcessor(),
                    new OfferEventContentGenerator(new MockKeyVaultUtils(), this._contentGeneratorLogger),
                    new MarketplaceOfferMapper(),
                    new MarketplacePlanMapper(),
                    new MarketplaceParameterMapper(),
                    new MarketplaceProvisioningStepMapper(),
                    context,
                    this._logger);

                var offerResponse = await function.CreateMarketplaceOfferAsync(this._offerRequest.OfferId, this._offerRequest, this._headers);
                Assert.IsInstanceOfType(offerResponse, typeof(MarketplaceOfferResponse));
                Assert.AreEqual(offerResponse.DisplayName, this._offerRequest.DisplayName);

                var paramResponse = await function.CreateParameterAsync(offerResponse.OfferId, this._paramRequest.ParameterName, this._paramRequest, this._headers);
                Assert.IsInstanceOfType(paramResponse, typeof(MarketplaceParameterResponse));
                Assert.AreEqual(this._paramRequest.DisplayName, paramResponse.DisplayName);

                paramResponse = await function.UpdateParameterAsync(offerResponse.OfferId, this._updatedParamRequest.ParameterName, this._updatedParamRequest, this._headers);
                Assert.IsInstanceOfType(paramResponse, typeof(MarketplaceParameterResponse));
                Assert.AreEqual(this._updatedParamRequest.DisplayName, paramResponse.DisplayName);

                paramResponse = await function.CreateParameterAsync(offerResponse.OfferId, this._secondParamRequest.ParameterName, this._secondParamRequest, this._headers);
                Assert.IsInstanceOfType(paramResponse, typeof(MarketplaceParameterResponse));
                Assert.AreEqual(this._secondParamRequest.DisplayName, paramResponse.DisplayName);

                var parameters = await function.ListParametersAsync(offerResponse.OfferId, this._headers);
                Assert.AreEqual(2, parameters.Count);

                await function.DeleteParameterAsync(offerResponse.OfferId, this._paramRequest.ParameterName, this._headers);
                parameters = await function.ListParametersAsync(offerResponse.OfferId, this._headers);
                Assert.AreEqual(1, parameters.Count);
            }
        }

        [TestMethod]
        public async Task BasicProvisioningStepTest()
        {
            var builder = new DbContextOptionsBuilder<SqlDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).
                ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore).Log(InMemoryEventId.TransactionIgnoredWarning));
            var options = builder.Options;
            using (var context = new SqlDbContext(options))
            {
                IMarketplaceFunctionsImpl function = new MarketplaceFunctionsImpl(
                    new OfferEventProcessor(),
                    new OfferEventContentGenerator(new MockKeyVaultUtils(), this._contentGeneratorLogger),
                    new MarketplaceOfferMapper(),
                    new MarketplacePlanMapper(),
                    new MarketplaceParameterMapper(),
                    new MarketplaceProvisioningStepMapper(),
                    context,
                    this._logger);

                var offerResponse = await function.CreateMarketplaceOfferAsync(this._offerRequest.OfferId, this._offerRequest, this._headers);
                Assert.IsInstanceOfType(offerResponse, typeof(MarketplaceOfferResponse));
                Assert.AreEqual(offerResponse.DisplayName, this._offerRequest.DisplayName);

                BaseProvisioningStepResponse stepResponse = await function.CreateProvisioningStepAsync(
                    offerResponse.OfferId, 
                    this._scriptStepRequest.Name, 
                    this._scriptStepRequest, 
                    this._headers);

                Assert.IsInstanceOfType(stepResponse, typeof(ScriptProvisioningStepResponse));
                Assert.AreEqual(this._scriptStepRequest.ScriptPackageUrl, ((ScriptProvisioningStepResponse)stepResponse).ScriptPackageUrl);
                Assert.AreEqual(this._scriptStepRequest.InputArguments.Count, ((ScriptProvisioningStepResponse)stepResponse).InputArguments.Count);

                stepResponse = await function.UpdateProvisioningStepAsync(
                    offerResponse.OfferId,
                    this._updatedScriptStepRequest.Name,
                    this._updatedScriptStepRequest,
                    this._headers);
                Assert.AreEqual(this._updatedScriptStepRequest.ScriptPackageUrl, ((ScriptProvisioningStepResponse)stepResponse).ScriptPackageUrl);
                Assert.AreEqual(this._updatedScriptStepRequest.InputArguments.Count, ((ScriptProvisioningStepResponse)stepResponse).InputArguments.Count);

                stepResponse = await function.GetProvisioningStepAsync(
                    offerResponse.OfferId,
                    this._updatedScriptStepRequest.Name,
                    this._headers);
                Assert.AreEqual(this._updatedScriptStepRequest.ScriptPackageUrl, ((ScriptProvisioningStepResponse)stepResponse).ScriptPackageUrl);
                Assert.AreEqual(this._updatedScriptStepRequest.InputArguments.Count, ((ScriptProvisioningStepResponse)stepResponse).InputArguments.Count);

                stepResponse = await function.CreateProvisioningStepAsync(
                    offerResponse.OfferId,
                    this._armStepRequest.Name,
                    this._armStepRequest,
                    this._headers);

                Assert.IsInstanceOfType(stepResponse, typeof(ARMTemplateProvisioningStepResponse));
                Assert.AreEqual(this._armStepRequest.TemplateUrl, ((ARMTemplateProvisioningStepResponse)stepResponse).TemplateUrl);
                Assert.AreEqual(this._armStepRequest.InputParameterNames.Count, ((ARMTemplateProvisioningStepResponse)stepResponse).InputParameterNames.Count);

                stepResponse = await function.CreateProvisioningStepAsync(
                    offerResponse.OfferId,
                    this._webhookStepRequest.Name,
                    this._webhookStepRequest,
                    this._headers);

                Assert.IsInstanceOfType(stepResponse, typeof(WebhookProvisioningStepResponse));
                Assert.AreEqual(this._webhookStepRequest.WebhookUrl, ((WebhookProvisioningStepResponse)stepResponse).WebhookUrl);
                Assert.AreEqual(this._webhookStepRequest.InputParameterNames.Count, ((WebhookProvisioningStepResponse)stepResponse).InputParameterNames.Count);

                var steps = await function.ListProvisioningStepsAsync(offerResponse.OfferId, this._headers);
                Assert.AreEqual(3, steps.Count);

                await function.DeleteProvisioningStepAsync(
                    offerResponse.OfferId,
                    this._updatedScriptStepRequest.Name,
                    this._headers);

                steps = await function.ListProvisioningStepsAsync(offerResponse.OfferId, this._headers);
                Assert.AreEqual(2, steps.Count);
            }
        }
    }
}
