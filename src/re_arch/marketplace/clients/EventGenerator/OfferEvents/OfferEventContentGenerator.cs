using Luna.Common.Utils;
using Luna.Marketplace.Data;
using Luna.Marketplace.Public.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Marketplace.Clients
{
    public class OfferEventContentGenerator : IOfferEventContentGenerator
    {
        private IAzureKeyVaultUtils _keyVaultUtils;
        private ILogger<OfferEventContentGenerator> _logger;

        public OfferEventContentGenerator(
            IAzureKeyVaultUtils keyVaultUtils,
            ILogger<OfferEventContentGenerator> logger)
        {
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate create marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer name</param>
        /// <param name="offerReq">The offer properties</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateCreateMarketplaceOfferEventContentAsync(
            string offerId,
            MarketplaceOfferProp offerProp)
        {
            var offer = new MarketplaceOffer
            {
                OfferId = offerId,
                Properties = offerProp,
                Status = MarketplaceOfferStatus.Draft.ToString(),
            };

            var ev = new CreateMarketplaceOfferEvent
            {
                OfferId = offerId,
                Offer = offer,
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate update marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer name</param>
        /// <param name="offerReq">The offer properties</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateUpdateMarketplaceOfferEventContentAsync(
            string offerId,
            MarketplaceOfferProp offerProp)
        {
            var offer = new MarketplaceOffer
            {
                OfferId = offerId,
                Properties = offerProp,
                Status = MarketplaceOfferStatus.Draft.ToString(),
            };

            var ev = new CreateMarketplaceOfferFromTemplateEvent
            {
                OfferId = offerId,
                Offer = offer,
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate create marketplace offer from template event and convert to JSON string
        /// </summary>
        /// <param name="template">The offer template</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateCreateMarketplaceOfferFromTemplateEventContentAsync(
            string template)
        {
            var offer = await ParseMarketplaceOfferFromTemplate(template);

            var ev = new CreateMarketplaceOfferFromTemplateEvent()
            {
                OfferId = offer.OfferId,
                Offer = offer
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate update marketplace offer from template event and convert to JSON string
        /// </summary>
        /// <param name="template">The offer template</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateUpdateMarketplaceOfferFromTemplateEventContentAsync(
            string template)
        {
            var offer = await ParseMarketplaceOfferFromTemplate(template);

            var ev = new UpdateMarketplaceOfferFromTemplateEvent()
            {
                OfferId = offer.OfferId,
                Offer = offer
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        private async Task<MarketplaceOffer> ParseMarketplaceOfferFromTemplate(string template)
        {
            MarketplaceOffer offer = JsonConvert.DeserializeObject<MarketplaceOffer>(template);

            if (offer.ProvisioningSteps.Count > 0)
            {
                var provisioningSteps = new List<MarketplaceProvisioningStep>();
                var rawOffer = JObject.Parse(template);

                if (rawOffer.ContainsKey("ProvisioningSteps"))
                {
                    JArray steps = (JArray)rawOffer["ProvisioningSteps"];
                    foreach (var step in steps)
                    {
                        object type = null;
                        BaseProvisioningStepProp stepProp = null;
                        if (Enum.TryParse(typeof(MarketplaceProvisioningStepType), step["Type"].ToString(), out type))
                        {
                            switch ((MarketplaceProvisioningStepType)type)
                            {
                                case MarketplaceProvisioningStepType.ARMTemplate:
                                    stepProp = JsonConvert.DeserializeObject<ARMTemplateProvisioningStepProp>(step["Properties"].ToString());
                                    break;
                                case MarketplaceProvisioningStepType.Script:
                                    stepProp = JsonConvert.DeserializeObject<ScriptProvisioningStepProp>(step["Properties"].ToString());
                                    break;
                            }
                        }

                        if (stepProp != null)
                        {
                            provisioningSteps.Add(new MarketplaceProvisioningStep
                            {
                                Name = step["Name"].ToString(),
                                Type = step["Type"].ToString(),
                                Properties = stepProp
                            });
                        }

                    }
                }
                offer.ProvisioningStepsSecretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.PROVISIONING_STEPS);
                var content = JsonConvert.SerializeObject(provisioningSteps, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                await this._keyVaultUtils.SetSecretAsync(offer.ProvisioningStepsSecretName, content);
            }

            return offer;
        }

        /// <summary>
        /// Generate delete marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer name</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateDeleteMarketplaceOfferEventContentAsync(
            string offerId)
        {
            var ev = new DeleteMarketplaceOfferEvent
            {
                OfferId = offerId,
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate publish marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer name</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GeneratePublishMarketplaceOfferEventContentAsync(
            string offerId)
        {
            var ev = new PublishMarketplaceOfferEvent
            {
                OfferId = offerId,
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate create marketplace plan event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan Id</param>
        /// <param name="planReq">The plan properties</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateCreateMarketplacePlanEventContentAsync(
            string offerId,
            string planId,
            MarketplacePlanProp planProp)
        {
            var plan = new MarketplacePlan
            {
                OfferId = offerId,
                PlanId = planId,
                Properties = planProp,
            };

            var ev = new CreateMarketplacePlanEvent
            {
                OfferId = offerId, 
                PlanId = planId,
                Plan = plan,
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate update marketplace plan event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan Id</param>
        /// <param name="planReq">The plan properties</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateUpdateMarketplacePlanEventContentAsync(
            string offerId,
            string planId,
            MarketplacePlanProp planProp)
        {
            var plan = new MarketplacePlan
            {
                OfferId = offerId,
                PlanId = planId,
                Properties = planProp,
            };

            var ev = new UpdateMarketplacePlanEvent
            {
                OfferId = offerId,
                PlanId = planId,
                Plan = plan,
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate delete marketplace plan event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="planId">The plan Id</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateDeleteMarketplacePlanEventContentAsync(
            string offerId,
            string planId)
        {
            var ev = new DeleteMarketplacePlanEvent
            {
                OfferId = offerId,
                PlanId = planId
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate create offer parameter event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="paramReq">The parameter properties</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateCreateOfferParameterEventContentAsync(
            string offerId,
            string parameterName,
            MarketplaceParameter param)
        {
            var ev = new CreateMarketplaceOfferParameterEvent
            {
                OfferId = offerId,
                ParameterName = parameterName,
                Parameter = param
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate update offer parameter event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="paramReq">The parameter properties</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateUpdateOfferParameterEventContentAsync(
            string offerId,
            string parameterName,
            MarketplaceParameter param)
        {
            var ev = new UpdateMarketplaceOfferParameterEvent
            {
                OfferId = offerId,
                ParameterName = parameterName,
                Parameter = param
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate delete offer parameter event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateDeleteOfferParameterEventContentAsync(
            string offerId,
            string parameterName)
        {
            var ev = new DeleteMarketplaceOfferParameterEvent
            {
                OfferId = offerId,
                ParameterName = parameterName,
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate create provisiong step event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="stepName">The provision step name</param>
        /// <param name="stepType">The type of the provisioning step</param>
        /// <param name="stepProp">The provision step properties</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateCreateProvisoningStepEventContentAsync(
            string offerId,
            string stepName,
            string stepType,
            BaseProvisioningStepProp stepProp)
        {
            var step = new MarketplaceProvisioningStep
            {
                Name = stepName,
                Type = stepType,
                Properties = stepProp
            };

            var ev = new CreateProvisioningStepEvent
            {
                OfferId = offerId,
                StepName = stepName,
                StepSecretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.PROVISIONING_STEPS),
            };

            var secret = JsonConvert.SerializeObject(step, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            await this._keyVaultUtils.SetSecretAsync(ev.StepSecretName, secret);

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate update provisiong step event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="stepName">The provision step name</param>
        /// <param name="stepType">The type of the provisioning step</param>
        /// <param name="stepProp">The provision step properties</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateUpdateProvisoningStepEventContentAsync(
            string offerId,
            string stepName,
            string stepType,
            BaseProvisioningStepProp stepProp)
        {
            var step = new MarketplaceProvisioningStep
            {
                Name = stepName,
                Type = stepType,
                Properties = stepProp
            };

            var ev = new UpdateProvisioningStepEvent
            {
                OfferId = offerId,
                StepName = stepName,
                StepSecretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.PROVISIONING_STEPS),
            };

            var secret = JsonConvert.SerializeObject(step, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            await this._keyVaultUtils.SetSecretAsync(ev.StepSecretName, secret);

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }

        /// <summary>
        /// Generate delete provisiong step event and convert to JSON string
        /// </summary>
        /// <param name="offerId">The offer id</param>
        /// <param name="stepName">The provision step name</param>
        /// <returns>The event content JSON string</returns>
        public async Task<string> GenerateDeleteProvisoningStepEventContentAsync(
            string offerId,
            string stepName)
        {
            var ev = new DeleteProvisioningStepEvent
            {
                OfferId = offerId,
                StepName = stepName,
            };

            var content = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return content;
        }
    }
}
