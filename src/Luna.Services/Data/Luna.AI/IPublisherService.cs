using Luna.Data.Entities;
using Luna.Data.Entities.Luna.AI;
using System.Threading.Tasks;

namespace Luna.Services.Data
{
    public interface IPublisherService
    {
        Task<Publisher> GetAsync();
    }
}
