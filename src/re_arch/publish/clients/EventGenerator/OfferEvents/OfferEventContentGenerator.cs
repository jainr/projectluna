using Luna.Common.Utils;
using Luna.Publish.Data;
using Luna.Publish.Public.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Publish.Clients
{
    public class OfferEventContentGenerator : IOfferEventContentGenerator
    {
        private IAzureKeyVaultUtils _keyVaultUtils;
        private ILogger<OfferEventContentGenerator> _logger;

        public OfferEventContentGenerator(IAzureKeyVaultUtils keyVaultUtils, ILogger<OfferEventContentGenerator> logger)
        {
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate create marketplace offer event and convert to JSON string
        /// </summary>
        /// <param name="name">The offer name</param>
        /// <param name="properties">The offer properties</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateCreateMarketplaceOfferEventContent(
            string name,
            MarketplaceOfferProp properties)
        {
            return "";
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
    }
}
