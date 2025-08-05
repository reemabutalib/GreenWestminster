using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Repositories.Interfaces
{
    public interface ISustainableEventRepository
    {
        Task<SustainabilityEvent?> GetByIdAsync(int id);
        Task<List<SustainabilityEvent>> GetAllAsync();
        Task AddAsync(SustainabilityEvent sustainabilityEvent);
        Task UpdateAsync(SustainabilityEvent sustainabilityEvent);
        Task DeleteAsync(int id);
    }
}