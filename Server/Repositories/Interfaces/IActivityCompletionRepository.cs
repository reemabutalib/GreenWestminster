using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Repositories.Interfaces
{

    public interface IActivityCompletionRepository
    {
        Task<ActivityCompletion?> GetByIdAsync(int id);
        Task<List<ActivityCompletion>> GetByUserIdAsync(int userId);
        Task<List<ActivityCompletion>> GetAllAsync();
        Task AddAsync(ActivityCompletion completion);
        Task UpdateAsync(ActivityCompletion completion);
        Task DeleteAsync(int id);
    }
}