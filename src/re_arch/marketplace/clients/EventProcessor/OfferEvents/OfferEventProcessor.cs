using Luna.Common.Utils;
using Luna.Marketplace.Data;
using Luna.Marketplace.Public.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Luna.Marketplace.Clients
{
    public class OfferEventProcessor : IOfferEventProcessor
    {

        private IAzureKeyVaultUtils _keyVaultUtils;

        public OfferEventProcessor(
            IAzureKeyVaultUtils keyVaultUtils)
        {
            this._keyVaultUtils = keyVaultUtils ?? throw new ArgumentNullException(nameof(keyVaultUtils));
        }

        /// <summary>
        /// Get marketplace offer from a snapshot and events
        /// </summary>
        /// <param name="offerId">The id of the offer</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        public async Task<MarketplaceOffer> GetMarketplaceOfferAsync(
            string offerId, 
            List<BaseMarketplaceEvent> events,
            MarketplaceOfferSnapshotDB snapshot = null)
        {
            MarketplaceOffer result = null;

            if (snapshot != null)
            {
                result = JsonConvert.DeserializeObject<MarketplaceOffer>(snapshot.SnapshotContent, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                var provisionStepsSecret = await this._keyVaultUtils.GetSecretAsync(result.ProvisioningStepsSecretName);

                result.ProvisioningSteps = JsonConvert.DeserializeObject<List<MarketplaceProvisioningStep>>(provisionStepsSecret, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

            }
            else if (events[0].EventType != MarketplaceEventType.CreateMarketplaceOffer && 
                events[0].EventType != MarketplaceEventType.CreateMarketplaceOfferFromTemplate)
            {
                throw new LunaServerException($"The snapshot of marketplace offer {offerId} is null.");
            }

            foreach (var ev in events)
            {
                switch (ev.EventType)
                {
                    case MarketplaceEventType.CreateMarketplaceOfferFromTemplate:
                        result = ((CreateMarketplaceOfferFromTemplateEvent)ev).Offer;
                        result.Status = MarketplaceOfferStatus.Draft.ToString();
                        break;
                    case MarketplaceEventType.UpdateMarketplaceOfferFromTemplate:
                        result = ((UpdateMarketplaceOfferFromTemplateEvent)ev).Offer;
                        break;
                    case MarketplaceEventType.PublishMarketplaceOffer:
                        result.Status = MarketplaceOfferStatus.Published.ToString();
                        break;
                    case MarketplaceEventType.DeleteMarketplaceOffer:
                        result = null;
                        break;
                    case MarketplaceEventType.CreateMarketplaceOffer:
                        result = ((CreateMarketplaceOfferEvent)ev).Offer;
                        result.Status = MarketplaceOfferStatus.Draft.ToString();
                        break;
                    case MarketplaceEventType.UpdateMarketplaceOffer:
                        result.Properties = ((UpdateMarketplaceOfferEvent)ev).Offer.Properties;
                        break;
                    case MarketplaceEventType.CreateMarketplacePlan:
                        result.Plans.Add(((CreateMarketplacePlanEvent)ev).Plan);
                        break;
                    case MarketplaceEventType.UpdateMarketplacePlan:
                        var updatePlanEv = (UpdateMarketplacePlanEvent)ev;
                        result.Plans.RemoveAll(x => x.PlanId == updatePlanEv.PlanId);
                        result.Plans.Add(updatePlanEv.Plan);
                        break;
                    case MarketplaceEventType.DeleteMarketplacePlan:
                        result.Plans.RemoveAll(x => x.PlanId == ((DeleteMarketplacePlanEvent)ev).PlanId);
                        break;
                    case MarketplaceEventType.CreateMarketplaceOfferParameter:
                        result.Parameters.Add(((CreateMarketplaceOfferParameterEvent)ev).Parameter);
                        break;
                    case MarketplaceEventType.UpdateMarketplaceOfferParameter:
                        var paramEv = (UpdateMarketplaceOfferParameterEvent)ev;
                        result.Parameters.RemoveAll(x => x.ParameterName == paramEv.ParameterName);
                        result.Parameters.Add(paramEv.Parameter);
                        break;
                    case MarketplaceEventType.DeleteMarketplaceOfferParameter:
                        result.Parameters.RemoveAll(x => x.ParameterName == ((DeleteMarketplaceOfferParameterEvent)ev).ParameterName);
                        break;
                    case MarketplaceEventType.CreateMarketplaceProvisioningStep:
                        var createStepEv = (CreateProvisioningStepEvent)ev;
                        var secret = await this._keyVaultUtils.GetSecretAsync(createStepEv.StepSecretName);
                        var step = JsonConvert.DeserializeObject<MarketplaceProvisioningStep>(secret, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                        });
                        result.ProvisioningSteps.Add(step);
                        break;
                    case MarketplaceEventType.UpdateMarketplaceProvisioningStep:
                        var updateStepEv = (UpdateProvisioningStepEvent)ev;
                        secret = await this._keyVaultUtils.GetSecretAsync(updateStepEv.StepSecretName);
                        step = JsonConvert.DeserializeObject<MarketplaceProvisioningStep>(secret, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                        });
                        result.ProvisioningSteps.RemoveAll(x => x.Name == updateStepEv.StepName);
                        result.ProvisioningSteps.Add(step);
                        break;
                    case MarketplaceEventType.DeleteMarketplaceProvisioningStep:
                        result.ProvisioningSteps.RemoveAll(x => x.Name == ((DeleteProvisioningStepEvent)ev).StepName);
                        break;
                    default:
                        throw new LunaServerException($"Unknown event type {ev.EventType.ToString()}.");
                }
            }

            return result;
        }

        /// <summary>
        /// Get marketplace offer in JSON string from a snapshot and events
        /// </summary>
        /// <param name="offerId">The id of the offer</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        public async Task<string> GetMarketplaceOfferJSONStringAsync(
            string offerId,
            List<BaseMarketplaceEvent> events,
            MarketplaceOfferSnapshotDB snapshot = null)
        {
            var offer = await GetMarketplaceOfferAsync(offerId, events, snapshot);

            offer.ProvisioningStepsSecretName = AzureKeyVaultUtils.GenerateSecretName(SecretNamePrefixes.PROVISIONING_STEPS);

            var secret = JsonConvert.SerializeObject(offer.ProvisioningSteps, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            await this._keyVaultUtils.SetSecretAsync(offer.ProvisioningStepsSecretName, secret);

            return JsonConvert.SerializeObject(offer, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}
