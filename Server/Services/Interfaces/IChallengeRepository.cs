using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Models;

public interface IChallengeRepository
{
    Task<List<Challenge>> GetAllAsync();
    Task<Challenge?> GetByIdAsync(int id);
    Task AddAsync(Challenge challenge);
    Task UpdateAsync(Challenge challenge);
    Task DeleteAsync(int id);
    Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId);
    Task<IEnumerable<UserChallenge>> GetCompletedChallengesByUserIdAsync(int userId);
}