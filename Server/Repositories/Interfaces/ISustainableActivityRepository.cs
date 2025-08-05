using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Repositories.Interfaces
{

    public interface ISustainableActivityRepository
    {
        Task<SustainableActivity?> GetByIdAsync(int id);
        Task<List<SustainableActivity>> GetAllAsync();
        Task AddAsync(SustainableActivity activity);
        Task UpdateAsync(SustainableActivity activity);
        Task DeleteAsync(int id);
    }
}