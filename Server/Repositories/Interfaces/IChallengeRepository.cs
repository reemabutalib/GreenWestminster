using Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Repositories.Interfaces
{
    public interface IChallengeRepository
    {
        Task<List<Challenge>> GetAllAsync();
        Task<Challenge?> GetByIdAsync(int id);
        Task AddAsync(Challenge challenge);
        Task UpdateAsync(Challenge challenge);
        Task DeleteAsync(int id);
        Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId);
        Task<IEnumerable<UserChallenge>> GetCompletedChallengesByUserIdAsync(int userId);
        Task AddUserChallengeAsync(UserChallenge userChallenge);
    }
}
