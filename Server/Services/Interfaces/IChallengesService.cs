using Server.Models;
using Server.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Services.Interfaces
{
    public interface IChallengesService
    {
        Task<IEnumerable<object>> GetChallengesAsync();
        Task<IEnumerable<object>> GetActiveChallengesAsync();
        Task<object?> GetChallengeByIdAsync(int id);
        Task<IEnumerable<object>> GetUserChallengesAsync(int userId);
        Task<object> CreateChallengeAsync(Challenge challenge);
        Task<bool> JoinChallengeAsync(int challengeId, int userId);
        Task<IEnumerable<object>> GetPastChallengesAsync();
        Task<object?> UpdateChallengeAsync(int id, Challenge challenge);
        Task<bool> DeleteChallengeAsync(int id);
        Task<object?> UpdateChallengeStatusAsync(int id, ChallengeStatusUpdateDto statusUpdate);
        Task<bool> AddActivitiesToChallengeAsync(int challengeId, List<int> activityIds);
    }
}
