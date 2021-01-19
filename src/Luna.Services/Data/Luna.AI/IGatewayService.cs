using Luna.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luna.Services.Data
{
    public interface IGatewayService
    {
        Task<List<Gateway>> GetAllAsync();

        Task<Gateway> GetAsync(string name);

        Task<Gateway> CreateAsync(Gateway gateway);

        Task<Gateway> UpdateAsync(string name, Gateway gateway);

        Task DeleteAsync(string name);

        Task<bool> ExistsAsync(string name);

        Task<Gateway> GetLeastUsedPublicGatewayAsync();
    }
}
