using Luna.Common.Utils;
using Luna.Partner.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Partner.Clients
{
    public interface IPartnerFunctionsImpl
    {
        Task<List<ServiceType>> GetMLHostServiceTypesAsync(LunaRequestHeaders headers);

        Task<List<ServiceType>> GetMLComputeServiceTypesAsync(LunaRequestHeaders headers);

        Task<List<ComponentType>> GetMLComputeServiceTypesAsync(string serviceType, LunaRequestHeaders headers);

        Task<BasePartnerServiceConfiguration> GetPartnerServiceAsync(string name, LunaRequestHeaders headers);

        Task<List<PartnerServiceOutlineResponse>> ListPartnerServicesByTypeAsync(string type, LunaRequestHeaders headers);

        Task<BasePartnerServiceConfiguration> AddPartnerServiceAsync(string name, BasePartnerServiceConfiguration configuration, LunaRequestHeaders headers);

        Task<BasePartnerServiceConfiguration> UpdatePartnerServiceAsync(string name, BasePartnerServiceConfiguration configuration, LunaRequestHeaders headers);

        Task RemovePartnerServiceAsync(string name, LunaRequestHeaders headers);
    }
}
