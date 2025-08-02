using Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.DTOs;

namespace Server.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User?> GetUserAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task<IEnumerable<User>> GetLeaderboardAsync(string timeFrame);
        Task<object?> GetUserStatsAsync(int id);
        Task<IEnumerable<object>> GetRecentActivitiesAsync(int id);
        Task<IEnumerable<object>> GetCompletedActivitiesAsync(int id, DateTime date);
        Task<IEnumerable<object>> GetCompletedChallengesAsync(int id);
        Task<object?> GetChallengeStatusAsync(int userId, int challengeId);
        Task<ActivityCompletion?> CompleteActivityAsync(int userId, int activityId);
        Task<object?> GetActivityStatsAsync(int userId);
        Task<IEnumerable<object>> GetPointsHistoryAsync(int userId);
        Task<object> AddPointsAsync(int userId, int points);
        Task<object?> GetLevelInfoAsync(int userId);
    }
}