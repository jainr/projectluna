using Luna.Common.Utils;
using Luna.Marketplace.Data;
using Luna.Marketplace.Public.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Marketplace.Clients
{
    public class MarketplaceFunctionsImpl : IMarketplaceFunctionsImpl
    {
        private readonly IOfferEventContentGenerator _offerEventGenerator;
        private readonly IOfferEventProcessor _offerEventProcessor;
        private readonly ISqlDbContext _dbContext;
        private readonly ILogger<MarketplaceFunctionsImpl> _logger;

        private IDataMapper<MarketplaceOfferRequest, MarketplaceOfferResponse, MarketplaceOfferProp> _offerDataMapper;
        private IDataMapper<MarketplacePlanRequest, MarketplacePlanResponse, MarketplacePlanProp> _planDataMapper;
        private IDataMapper<MarketplaceParameterRequest, MarketplaceParameterResponse, MarketplaceParameter> _parameterDataMapper;
        private IDataMapper<BaseProvisioningStepRequest, BaseProvisioningStepResponse, BaseProvisioningStepProp> _provisioningStepDataMapper;

        public MarketplaceFunctionsImpl(
            IOfferEventProcessor offerEventProcessor,
            IOfferEventContentGenerator offerEventContentGenerator,
            IDataMapper<MarketplaceOfferRequest, MarketplaceOfferResponse, MarketplaceOfferProp> offerDataMapper,
            IDataMapper<MarketplacePlanRequest, MarketplacePlanResponse, MarketplacePlanProp> planDataMapper,
            IDataMapper<MarketplaceParameterRequest, MarketplaceParameterResponse, MarketplaceParameter> parameterDataMapper,
            IDataMapper<BaseProvisioningStepRequest, BaseProvisioningStepResponse, BaseProvisioningStepProp> provisioningStepDataMappter,
            ISqlDbContext dbContext,
            ILogger<MarketplaceFunctionsImpl> logger)
        {
            this._offerEventProcessor = offerEventProcessor ?? throw new ArgumentNullException(nameof(offerEventProcessor));
            this._offerEventGenerator = offerEventContentGenerator ?? throw new ArgumentNullException(nameof(offerEventContentGenerator));
            this._planDataMapper = planDataMapper ?? throw new ArgumentNullException(nameof(planDataMapper));
            this._offerDataMapper = offerDataMapper ?? throw new ArgumentNullException(nameof(offerDataMapper));
            this._parameterDataMapper = parameterDataMapper ?? throw new ArgumentNullException(nameof(parameterDataMapper));
            this._provisioningStepDataMapper = provisioningStepDataMappter ?? throw new ArgumentNullException(nameof(provisioningStepDataMappter));
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MarketplaceOfferResponse> CreateMarketplaceOfferAsync(string offerId, MarketplaceOfferRequest offer, LunaRequestHeaders headers)
        {
            if (offer.OfferId != offerId)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.MARKETPLACE_OFFER_NAME_DOES_NOT_MATCH, offerId, offer.OfferId),
                    UserErrorCode.NameMismatch, target: nameof(offerId));
            }

            var offerDb = await _dbContext.MarketplaceOffers.
                SingleOrDefaultAsync(x => x.OfferId == offerId &&
                x.Status != MarketplaceOfferStatus.Deleted.ToString());

            if (offerDb != null)
            {
                throw new LunaConflictUserException(string.Format(ErrorMessages.MARKETPLACE_OFFER_ALREADY_EXIST, offerId));
            }

            var offerProp = this._offerDataMapper.Map(offer);

            var offerEvent = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                CreatedTime = DateTime.UtcNow,
                EventType = MarketplaceEventType.CreateMarketplaceOffer.ToString(),
                EventContent = await _offerEventGenerator.GenerateCreateMarketplaceOfferEventContentAsync(offerId, offerProp)
            };

            var snapshot = await this.CreateMarketplaceOfferSnapshotAsync(offerId,
                    MarketplaceOfferStatus.Draft,
                    offerEvent,
                    "",
                    true);

            var createdTime = DateTime.UtcNow;
            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(offerEvent);
                await _dbContext._SaveChangesAsync();

                _dbContext.MarketplaceOffers.Add(
                    new MarketplaceOfferDB
                    {
                        OfferId = offerId,
                        DisplayName = offerProp.DisplayName,
                        Description = offerProp.Description,
                        Status = MarketplaceOfferStatus.Draft.ToString(),
                        CreatedBy = headers.UserId,
                        CreatedTime = createdTime,
                        LastUpdatedTime = createdTime
                    });
                await _dbContext._SaveChangesAsync();

                if (snapshot != null)
                {
                    snapshot.LastAppliedEventId = offerEvent.Id;
                    _dbContext.MarketplaceOfferSnapshots.Add(snapshot);
                    await _dbContext._SaveChangesAsync();
                }
                else
                {
                    throw new LunaServerException($"Snapshot for offer {offerId} does not exist.");
                }

                transaction.Commit();
            }

            var response = this._offerDataMapper.Map(offerProp);

            response.CreatedTime = createdTime;
            response.LastUpdatedTime = createdTime;
            response.OfferId = offerId;
            response.Status = MarketplaceOfferStatus.Draft.ToString();

            return response;
        }

        public async Task<MarketplaceOfferResponse> UpdateMarketplaceOfferAsync(string offerId, MarketplaceOfferRequest offer, LunaRequestHeaders headers)
        {
            if (offer.OfferId != offerId)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.MARKETPLACE_OFFER_NAME_DOES_NOT_MATCH, offerId, offer.OfferId),
                    UserErrorCode.NameMismatch, target: nameof(offerId));
            }

            var offerDb = await _dbContext.MarketplaceOffers.
                SingleOrDefaultAsync(x => x.OfferId == offerId &&
                x.Status != MarketplaceOfferStatus.Deleted.ToString());

            if (offerDb == null)
            {
                throw new LunaConflictUserException(string.Format(ErrorMessages.MARKETPLACE_OFFER_DOES_NOT_EXIST, offerId));
            }

            var offerProp = this._offerDataMapper.Map(offer);

            var offerEvent = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                CreatedTime = DateTime.UtcNow,
                EventType = MarketplaceEventType.UpdateMarketplaceOffer.ToString(),
                EventContent = await _offerEventGenerator.GenerateUpdateMarketplaceOfferEventContentAsync(offerId, offerProp)
            };

            offerDb.LastUpdatedTime = DateTime.UtcNow;
            offerDb.Description = offerProp.Description;
            offerDb.DisplayName = offerProp.DisplayName;
            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(offerEvent);
                await _dbContext._SaveChangesAsync();

                _dbContext.MarketplaceOffers.Update(offerDb);
                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            var response = this._offerDataMapper.Map(offerProp);

            response.CreatedTime = offerDb.CreatedTime;
            response.LastUpdatedTime = offerDb.LastUpdatedTime;
            response.LastPublishedTime = offerDb.LastPublishedTime;
            response.OfferId = offerId;
            response.Status = offerDb.Status;

            return response;
        }

        public async Task PublishMarketplaceOfferAsync(string offerId, LunaRequestHeaders headers)
        {
            var offerDb = await _dbContext.MarketplaceOffers.
                SingleOrDefaultAsync(x => x.OfferId == offerId &&
                x.Status != MarketplaceOfferStatus.Deleted.ToString());

            if (offerDb == null)
            {
                throw new LunaConflictUserException(string.Format(ErrorMessages.MARKETPLACE_OFFER_DOES_NOT_EXIST, offerId));
            }

            var offerEvent = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                CreatedTime = DateTime.UtcNow,
                EventType = MarketplaceEventType.UpdateMarketplaceOffer.ToString(),
                EventContent = await _offerEventGenerator.GeneratePublishMarketplaceOfferEventContentAsync(offerId)
            };

            offerDb.LastUpdatedTime = DateTime.UtcNow;
            offerDb.LastPublishedTime = offerDb.LastUpdatedTime;
            offerDb.Status = MarketplaceOfferStatus.Published.ToString();
            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(offerEvent);
                await _dbContext._SaveChangesAsync();

                _dbContext.MarketplaceOffers.Update(offerDb);
                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            return;
        }

        public async Task<MarketplaceOfferResponse> GetMarketplaceOfferAsync(string offerId, LunaRequestHeaders headers)
        {
            var offerDb = await _dbContext.MarketplaceOffers.
                SingleOrDefaultAsync(x => x.OfferId == offerId &&
                x.Status != MarketplaceOfferStatus.Deleted.ToString());

            if (offerDb == null)
            {
                throw new LunaNotFoundUserException(string.Format(ErrorMessages.MARKETPLACE_OFFER_DOES_NOT_EXIST, offerId));
            }

            var offer = await this.GetMarketplaceOfferInternalAsync(offerId);
            var response = this._offerDataMapper.Map(offer.Properties);

            response.CreatedTime = offerDb.CreatedTime;
            response.LastUpdatedTime = offerDb.LastUpdatedTime;
            response.LastPublishedTime = offerDb.LastPublishedTime;
            response.OfferId = offerId;
            response.Status = offerDb.Status;

            return response;
        }

        public async Task<List<MarketplaceOfferResponse>> ListMarketplaceOffersAsync(string userId, LunaRequestHeaders headers)
        {
            var offersDb = await _dbContext.MarketplaceOffers.
                Where(x => x.CreatedBy == userId &&
                x.Status != MarketplaceOfferStatus.Deleted.ToString()).ToListAsync();

            List<MarketplaceOfferResponse> offers = new List<MarketplaceOfferResponse>();

            foreach (var offer in offersDb)
            {
                offers.Add(new MarketplaceOfferResponse 
                { 
                    OfferId = offer.OfferId,
                    DisplayName = offer.DisplayName,
                    Description = offer.Description,
                    CreatedTime = offer.CreatedTime,
                    Status = offer.Status,
                    LastUpdatedTime = offer.LastUpdatedTime
                });
            }

            return offers;
        }

        public async Task DeleteMarketplaceOfferAsync(string offerId, LunaRequestHeaders headers)
        {
            var offerDb = await _dbContext.MarketplaceOffers.
                SingleOrDefaultAsync(x => x.OfferId == offerId &&
                x.Status != MarketplaceOfferStatus.Deleted.ToString());

            if (offerDb == null)
            {
                throw new LunaConflictUserException(string.Format(ErrorMessages.MARKETPLACE_OFFER_DOES_NOT_EXIST, offerId));
            }
            
            var offerEvent = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                CreatedTime = DateTime.UtcNow,
                EventType = MarketplaceEventType.DeleteMarketplaceOffer.ToString(),
                EventContent = await _offerEventGenerator.GenerateDeleteMarketplaceOfferEventContentAsync(offerId)
            };

            var snapshot = await this.CreateMarketplaceOfferSnapshotAsync(offerId,
                    MarketplaceOfferStatus.Draft,
                    null,
                    "",
                    false);

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(offerEvent);
                await _dbContext._SaveChangesAsync();

                _dbContext.MarketplaceOffers.Remove(offerDb);
                await _dbContext._SaveChangesAsync();

                if (snapshot != null)
                {
                    snapshot.LastAppliedEventId = offerEvent.Id;
                    _dbContext.MarketplaceOfferSnapshots.Add(snapshot);
                    await _dbContext._SaveChangesAsync();
                }
                else
                {
                    throw new LunaServerException($"Snapshot for offer {offerId} does not exist.");
                }

                transaction.Commit();
            }

            return;

        }

        public async Task<MarketplacePlanResponse> CreateMarketplacePlanAsync(string offerId, string planId, MarketplacePlanRequest plan, LunaRequestHeaders headers)
        {
            if (plan.PlanId != planId)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.MARKETPLACE_PLAN_NAME_DOES_NOT_MATCH, planId, plan.PlanId),
                    UserErrorCode.NameMismatch, target: nameof(planId));
            }

            var planDb = await _dbContext.MarketplacePlans.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.PlanId == planId);

            if (planDb != null)
            {
                throw new LunaConflictUserException(string.Format(ErrorMessages.MARKETPLACE_PLAN_ALREADY_EXIST, planId, offerId));
            }

            var planProp = this._planDataMapper.Map(plan);
            var ev = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                EventType = MarketplaceEventType.CreateMarketplacePlan.ToString(),
                EventContent = await _offerEventGenerator.GenerateCreateMarketplacePlanEventContentAsync(offerId, planId, planProp),
                CreatedTime = DateTime.UtcNow
            };

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(ev);
                await _dbContext._SaveChangesAsync();

                var createdTime = DateTime.UtcNow;
                _dbContext.MarketplacePlans.Add(
                    new MarketplacePlanDB
                    {
                        OfferId = offerId,
                        PlanId = planId,
                        DisplayName = plan.DisplayName,
                        Description = plan.Description,
                        Mode = plan.Mode,
                        CreatedTime = createdTime,
                        LastUpdatedTime = createdTime
                    });
                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            var response = this._planDataMapper.Map(planProp);

            return response;
        }

        public async Task<MarketplacePlanResponse> UpdateMarketplacePlanAsync(string offerId, string planId, MarketplacePlanRequest plan, LunaRequestHeaders headers)
        {
            if (plan.PlanId != planId)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.MARKETPLACE_PLAN_NAME_DOES_NOT_MATCH, planId, plan.PlanId),
                    UserErrorCode.NameMismatch, target: nameof(planId));
            }

            var planDb = await _dbContext.MarketplacePlans.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.PlanId == planId);

            if (planDb == null)
            {
                throw new LunaConflictUserException(string.Format(ErrorMessages.MARKETPLACE_PLAN_DOES_NOT_EXIST, planId, offerId));
            }

            var planProp = this._planDataMapper.Map(plan);
            var ev = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                EventType = MarketplaceEventType.UpdateMarketplacePlan.ToString(),
                EventContent = await _offerEventGenerator.GenerateUpdateMarketplacePlanEventContentAsync(offerId, planId, planProp),
                CreatedTime = DateTime.UtcNow
            };

            planDb.LastUpdatedTime = DateTime.UtcNow;
            planDb.DisplayName = planProp.DisplayName;
            planDb.Description = planProp.Description;
            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(ev);
                await _dbContext._SaveChangesAsync();

                _dbContext.MarketplacePlans.Update(planDb);
                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            var response = this._planDataMapper.Map(planProp);

            return response;
        }

        public async Task<MarketplacePlanResponse> GetMarketplacePlanAsync(string offerId, string planId, LunaRequestHeaders headers)
        {
            var planDb = await _dbContext.MarketplacePlans.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.PlanId == planId);

            if (planDb == null)
            {
                throw new LunaNotFoundUserException(string.Format(ErrorMessages.MARKETPLACE_PLAN_DOES_NOT_EXIST, planId, offerId));
            }

            var offer = await this.GetMarketplaceOfferInternalAsync(offerId);
            var response = this._planDataMapper.Map(offer.Plans.SingleOrDefault(x => x.PlanId == planId).Properties);

            response.CreatedTime = planDb.CreatedTime;
            response.LastUpdatedTime = planDb.LastUpdatedTime;
            response.OfferId = offerId;

            return response;
        }

        public async Task<List<MarketplacePlanResponse>> ListMarketplacePlansAsync(string offerId, LunaRequestHeaders headers)
        {
            var plansDb = await _dbContext.MarketplacePlans.Where(x => x.OfferId == offerId).ToListAsync();

            List<MarketplacePlanResponse> plans = new List<MarketplacePlanResponse>();

            foreach (var plan in plansDb)
            {
                plans.Add(new MarketplacePlanResponse
                {
                    OfferId = offerId,
                    PlanId = plan.PlanId,
                    DisplayName = plan.DisplayName,
                    Description = plan.Description,
                    Mode = plan.Mode,
                    CreatedTime = plan.CreatedTime,
                    LastUpdatedTime = plan.LastUpdatedTime
                });
            }

            return plans;
        }

        public async Task DeleteMarketplacePlanAsync(string offerId, string planId, LunaRequestHeaders headers)
        {
            var planDb = await _dbContext.MarketplacePlans.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.PlanId == planId);

            if (planDb == null)
            {
                throw new LunaConflictUserException(string.Format(ErrorMessages.MARKETPLACE_PLAN_DOES_NOT_EXIST, planId, offerId));
            }

            var ev = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                EventType = MarketplaceEventType.DeleteMarketplacePlan.ToString(),
                EventContent = await _offerEventGenerator.GenerateDeleteMarketplacePlanEventContentAsync(offerId, planId),
                CreatedTime = DateTime.UtcNow
            };

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(ev);
                await _dbContext._SaveChangesAsync();

                _dbContext.MarketplacePlans.Remove(planDb);
                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }
        }

        public async Task<MarketplaceParameterResponse> CreateParameterAsync(string offerId, string parameterName, MarketplaceParameterRequest parameter, LunaRequestHeaders headers)
        {
            if (parameter.ParameterName != parameterName)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.MARKETPLACE_PARAMETER_NAME_DOES_NOT_MATCH, parameterName, parameter.ParameterName),
                    UserErrorCode.NameMismatch, target: nameof(parameterName));
            }

            var paramDb = await _dbContext.MarketplaceParameters.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.ParameterName == parameterName);

            if (paramDb != null)
            {
                throw new LunaConflictUserException(
                    string.Format(ErrorMessages.MARKETPLACE_PARAMETER_ALREADY_EXIST, parameterName, offerId));
            }

            var parameterProp = this._parameterDataMapper.Map(parameter);

            var ev = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                EventType = MarketplaceEventType.CreateMarketplaceOfferParameter.ToString(),
                EventContent = await _offerEventGenerator.GenerateCreateOfferParameterEventContentAsync(offerId, parameterName, parameterProp),
                CreatedTime = DateTime.UtcNow
            };

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(ev);
                await _dbContext._SaveChangesAsync();

                var createdTime = DateTime.UtcNow;
                _dbContext.MarketplaceParameters.Add(
                    new MarketplaceParameterDB
                    {
                        OfferId = offerId,
                        ParameterName = parameterName,
                        DisplayName = parameter.DisplayName,
                        Description = parameter.Description,
                        CreatedTime = createdTime,
                        LastUpdatedTime = createdTime
                    });
                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            var response = this._parameterDataMapper.Map(parameterProp);

            return response;
        }

        public async Task<MarketplaceParameterResponse> UpdateParameterAsync(string offerId, string parameterName, MarketplaceParameterRequest parameter, LunaRequestHeaders headers)
        {
            if (parameter.ParameterName != parameterName)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.MARKETPLACE_PARAMETER_NAME_DOES_NOT_MATCH, parameterName, parameter.ParameterName),
                    UserErrorCode.NameMismatch, target: nameof(parameterName));
            }

            var paramDb = await _dbContext.MarketplaceParameters.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.ParameterName == parameterName);

            if (paramDb == null)
            {
                throw new LunaConflictUserException(
                    string.Format(ErrorMessages.MARKETPLACE_PARAMETER_DOES_NOT_EXIST, parameterName, offerId));
            }

            var parameterProp = this._parameterDataMapper.Map(parameter);

            var ev = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                EventType = MarketplaceEventType.UpdateMarketplaceOfferParameter.ToString(),
                EventContent = await _offerEventGenerator.GenerateUpdateOfferParameterEventContentAsync(offerId, parameterName, parameterProp),
                CreatedTime = DateTime.UtcNow
            };

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(ev);
                await _dbContext._SaveChangesAsync();

                paramDb.DisplayName = parameterProp.DisplayName;
                paramDb.Description = parameterProp.Description;
                paramDb.LastUpdatedTime = DateTime.UtcNow;
                _dbContext.MarketplaceParameters.Update(paramDb);
                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            var response = this._parameterDataMapper.Map(parameterProp);

            return response;
        }

        public async Task<MarketplaceParameterResponse> GetParameterAsync(string offerId, string parameterName, LunaRequestHeaders headers)
        {
            var paramDb = await _dbContext.MarketplaceParameters.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.ParameterName == parameterName);

            if (paramDb == null)
            {
                throw new LunaNotFoundUserException(
                    string.Format(ErrorMessages.MARKETPLACE_PARAMETER_DOES_NOT_EXIST, parameterName, offerId));
            }

            var offer = await this.GetMarketplaceOfferInternalAsync(offerId);
            var response = this._parameterDataMapper.Map(offer.Parameters.SingleOrDefault(x => x.ParameterName == parameterName));

            response.CreatedTime = paramDb.CreatedTime;
            response.LastUpdatedTime = paramDb.LastUpdatedTime;
            response.OfferId = offerId;

            return response;
        }

        public async Task<List<MarketplaceParameterResponse>> ListParametersAsync(string offerId, LunaRequestHeaders headers)
        {
            var paramsDb = await _dbContext.MarketplaceParameters.Where(x => x.OfferId == offerId).ToListAsync();

            List<MarketplaceParameterResponse> parameters = new List<MarketplaceParameterResponse>();

            foreach(var param in paramsDb)
            {
                parameters.Add(new MarketplaceParameterResponse
                {
                    OfferId = offerId,
                    DisplayName = param.DisplayName,
                    Description = param.Description,
                    ParameterName = param.ParameterName,
                    CreatedTime = param.CreatedTime,
                    LastUpdatedTime = param.LastUpdatedTime
                });
            }

            return parameters;
        }

        public async Task DeleteParameterAsync(string offerId, string parameterName, LunaRequestHeaders headers)
        {
            var paramDb = await _dbContext.MarketplaceParameters.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.ParameterName == parameterName);

            if (paramDb == null)
            {
                throw new LunaConflictUserException(
                    string.Format(ErrorMessages.MARKETPLACE_PARAMETER_DOES_NOT_EXIST, parameterName, offerId));
            }

            var ev = new MarketplaceEventDB()
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                EventType = MarketplaceEventType.DeleteMarketplaceOfferParameter.ToString(),
                EventContent = await _offerEventGenerator.GenerateDeleteOfferParameterEventContentAsync(offerId, parameterName),
                CreatedTime = DateTime.UtcNow
            };

            using (var transaction = await _dbContext.BeginTransactionAsync())
            {
                _dbContext.MarketplaceEvents.Add(ev);
                await _dbContext._SaveChangesAsync();

                var createdTime = DateTime.UtcNow;
                _dbContext.MarketplaceParameters.Remove(paramDb);
                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            return;
        }

        public async Task<BaseProvisioningStepResponse> CreateProvisioningStepAsync(string offerId, string stepName, BaseProvisioningStepRequest step, LunaRequestHeaders headers)
        {
            if (step.Name != stepName)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.MARKETPLACE_STEP_NAME_DOES_NOT_MATCH, stepName, step.Name),
                    UserErrorCode.NameMismatch, target: nameof(stepName));
            }

            var stepDb = await this._dbContext.MarketplaceProvisioningSteps.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.StepName == stepName);

            if (stepDb != null)
            {
                throw new LunaConflictUserException(
                    string.Format(ErrorMessages.MARKETPLACE_STEP_ALREADY_EXIST, stepName, offerId));
            }

            var stepProp = this._provisioningStepDataMapper.Map(step);

            var ev = new MarketplaceEventDB
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                EventType = MarketplaceEventType.CreateMarketplaceProvisioningStep.ToString(),
                EventContent = await _offerEventGenerator.GenerateCreateProvisoningStepEventContentAsync(offerId, stepName, step.Type, stepProp),
                CreatedTime = DateTime.UtcNow
            };

            using (var transaction = await this._dbContext.BeginTransactionAsync())
            {
                this._dbContext.MarketplaceEvents.Add(ev);
                await _dbContext._SaveChangesAsync();

                var createdTime = DateTime.UtcNow;
                this._dbContext.MarketplaceProvisioningSteps.Add(
                    new MarketplaceProvisioningStepDB
                    {
                        OfferId = offerId,
                        StepName = stepName,
                        Description = step.Description,
                        Type = step.Type,
                        CreatedTime = createdTime,
                        LastUpdatedTime = createdTime
                    });

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            var response = this._provisioningStepDataMapper.Map(stepProp);
            return response;
        }

        public async Task<BaseProvisioningStepResponse> UpdateProvisioningStepAsync(string offerId, string stepName, BaseProvisioningStepRequest step, LunaRequestHeaders headers)
        {
            if (step.Name != stepName)
            {
                throw new LunaBadRequestUserException(
                    string.Format(ErrorMessages.MARKETPLACE_STEP_NAME_DOES_NOT_MATCH, stepName, step.Name),
                    UserErrorCode.NameMismatch, target: nameof(stepName));
            }

            var stepDb = await this._dbContext.MarketplaceProvisioningSteps.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.StepName == stepName);

            if (stepDb == null)
            {
                throw new LunaNotFoundUserException(
                    string.Format(ErrorMessages.MARKETPLACE_STEP_DOES_NOT_EXIST, stepName, offerId));
            }

            var stepProp = this._provisioningStepDataMapper.Map(step);

            var ev = new MarketplaceEventDB
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                EventType = MarketplaceEventType.UpdateMarketplaceProvisioningStep.ToString(),
                EventContent = await _offerEventGenerator.GenerateUpdateProvisoningStepEventContentAsync(offerId, stepName, step.Type, stepProp),
                CreatedTime = DateTime.UtcNow
            };

            stepDb.LastUpdatedTime = DateTime.UtcNow;
            stepDb.Description = stepProp.Description;

            using (var transaction = await this._dbContext.BeginTransactionAsync())
            {
                this._dbContext.MarketplaceEvents.Add(ev);
                await _dbContext._SaveChangesAsync();

                this._dbContext.MarketplaceProvisioningSteps.Update(stepDb);

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }

            var response = this._provisioningStepDataMapper.Map(stepProp);
            return response;
        }

        public async Task<BaseProvisioningStepResponse> GetProvisioningStepAsync(string offerId, string stepName, LunaRequestHeaders headers)
        {
            var stepDb = await this._dbContext.MarketplaceProvisioningSteps.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.StepName == stepName);

            if (stepDb == null)
            {
                throw new LunaConflictUserException(
                    string.Format(ErrorMessages.MARKETPLACE_STEP_DOES_NOT_EXIST, stepName, offerId));
            }

            var offer = await this.GetMarketplaceOfferInternalAsync(offerId);
            var response = this._provisioningStepDataMapper.Map(
                offer.ProvisioningSteps.SingleOrDefault(x => x.Name == stepName).Properties);

            response.CreatedTime = stepDb.CreatedTime;
            response.LastUpdatedTime = stepDb.LastUpdatedTime;
            response.OfferId = offerId;

            return response;
        }
        public async Task<List<BaseProvisioningStepResponse>> ListProvisioningStepsAsync(string offerId, LunaRequestHeaders headers)
        {
            var stepsDb = await _dbContext.MarketplaceProvisioningSteps.Where(x => x.OfferId == offerId).ToListAsync();

            List<BaseProvisioningStepResponse> parameters = new List<BaseProvisioningStepResponse>();

            foreach (var step in stepsDb)
            {
                parameters.Add(new BaseProvisioningStepResponse
                {
                    OfferId = offerId,
                    Description = step.Description,
                    Name = step.StepName,
                    Type = step.Type,
                    CreatedTime = step.CreatedTime,
                    LastUpdatedTime = step.LastUpdatedTime
                });
            }

            return parameters;
        }

        public async Task DeleteProvisioningStepAsync(string offerId, string stepName, LunaRequestHeaders headers)
        {
            var stepDb = await this._dbContext.MarketplaceProvisioningSteps.
                SingleOrDefaultAsync(x => x.OfferId == offerId && x.StepName == stepName);

            if (stepDb == null)
            {
                throw new LunaNotFoundUserException(
                    string.Format(ErrorMessages.MARKETPLACE_STEP_DOES_NOT_EXIST, stepName, offerId));
            }

            var ev = new MarketplaceEventDB
            {
                EventId = Guid.NewGuid(),
                ResourceName = offerId,
                CreatedBy = headers.UserId,
                Tags = "",
                EventType = MarketplaceEventType.DeleteMarketplaceProvisioningStep.ToString(),
                EventContent = await _offerEventGenerator.GenerateDeleteProvisoningStepEventContentAsync(offerId, stepName),
                CreatedTime = DateTime.UtcNow
            };

            using (var transaction = await this._dbContext.BeginTransactionAsync())
            {
                this._dbContext.MarketplaceEvents.Add(ev);
                await _dbContext._SaveChangesAsync();

                var createdTime = DateTime.UtcNow;
                this._dbContext.MarketplaceProvisioningSteps.Remove(stepDb);

                await _dbContext._SaveChangesAsync();

                transaction.Commit();
            }
        }

        #region Private methods

        private async Task<MarketplaceOfferSnapshotDB> CreateMarketplaceOfferSnapshotAsync(string offerId,
            MarketplaceOfferStatus status,
            MarketplaceEventDB currentEvent = null,
            string tags = "",
            bool isNewOffer = false)
        {
            var snapshot = isNewOffer ? null : _dbContext.MarketplaceOfferSnapshots.
                Where(x => x.OfferId == offerId).
                OrderByDescending(x => x.LastAppliedEventId).FirstOrDefault();

            var events = new List<BaseMarketplaceEvent>();

            if (!isNewOffer)
            {
                events = await _dbContext.MarketplaceEvents.
                    Where(x => x.Id > snapshot.LastAppliedEventId && x.ResourceName == offerId).
                    OrderBy(x => x.Id).
                    Select(x => x.GetEventObject()).
                    ToListAsync();
            }

            if (currentEvent != null)
            {
                events.Add(currentEvent.GetEventObject());
            }

            var newSnapshot = new MarketplaceOfferSnapshotDB()
            {
                SnapshotId = Guid.NewGuid(),
                OfferId = offerId,
                SnapshotContent = _offerEventProcessor.GetMarketplaceOfferJSONString(offerId, events, snapshot),
                Status = status.ToString(),
                Tags = "",
                CreatedTime = DateTime.UtcNow,
                DeletedTime = null
            };

            return newSnapshot;
        }

        private async Task<MarketplaceOffer> GetMarketplaceOfferInternalAsync(string offerId)
        {
            var snapshot = await _dbContext.MarketplaceOfferSnapshots.
                Where(x => x.OfferId == offerId).
                OrderByDescending(x => x.LastAppliedEventId).FirstOrDefaultAsync();

            var events = await _dbContext.MarketplaceEvents.
                Where(x => x.Id > snapshot.LastAppliedEventId && x.ResourceName == offerId).
                OrderBy(x => x.Id).
                Select(x => x.GetEventObject()).
                ToListAsync();

            var offer = _offerEventProcessor.GetMarketplaceOffer(offerId, events, snapshot);

            return offer;
        }

        #endregion
    }
}
