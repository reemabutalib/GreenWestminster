using System.Threading.Tasks;
using Server.Models;
using Server.DTOs;

namespace Server.Services.Interfaces
{
    public interface IActivitiesService
    {
        Task<List<SustainableActivity>> GetAllActivitiesAsync();
        Task<SustainableActivity?> GetActivityByIdAsync(int id);
        Task<List<SustainableActivity>> GetActivitiesByCategoryAsync(string category);
        Task<List<SustainableActivity>> GetDailyActivitiesAsync();
        Task<List<SustainableActivity>> GetWeeklyActivitiesAsync();
        Task<List<SustainableActivity>> GetActivitiesByPointsRangeAsync(int min, int max);
        Task<SustainableActivity> CreateActivityAsync(SustainableActivity activity);
        Task<bool> UpdateActivityAsync(SustainableActivity activity);
        Task<bool> DeleteActivityAsync(int id);
        Task<bool> ActivityExists(int id);
        Task<List<ActivityCompletion>> GetCompletionsForUserAsync(int userId);
    }
}