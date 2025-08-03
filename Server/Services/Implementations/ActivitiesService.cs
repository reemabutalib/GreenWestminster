using Server.Models;
using Server.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Services.Implementations
{
    public class ActivitiesService : IActivitiesService
    {
        private readonly ISustainableActivityRepository _activityRepository;
        private readonly IActivityCompletionRepository _activityCompletionRepository;

        public ActivitiesService(
            ISustainableActivityRepository activityRepository,
            IActivityCompletionRepository activityCompletionRepository)
        {
            _activityRepository = activityRepository;
            _activityCompletionRepository = activityCompletionRepository;
        }

        public async Task<List<SustainableActivity>> GetAllActivitiesAsync()
        {
            return await _activityRepository.GetAllAsync();
        }

        public async Task<SustainableActivity?> GetActivityByIdAsync(int id)
        {
            return await _activityRepository.GetByIdAsync(id);
        }

        public async Task<List<SustainableActivity>> GetActivitiesByCategoryAsync(string category)
        {
            var all = await _activityRepository.GetAllAsync();
            return all.Where(a => a.Category == category).ToList();
        }

        public async Task<List<SustainableActivity>> GetDailyActivitiesAsync()
        {
            var all = await _activityRepository.GetAllAsync();
            return all.Where(a => a.IsDaily).ToList();
        }

        public async Task<List<SustainableActivity>> GetWeeklyActivitiesAsync()
        {
            var all = await _activityRepository.GetAllAsync();
            return all.Where(a => a.IsWeekly).ToList();
        }

        public async Task<List<SustainableActivity>> GetActivitiesByPointsRangeAsync(int min, int max)
        {
            var all = await _activityRepository.GetAllAsync();
            return all.Where(a => a.PointsValue >= min && a.PointsValue <= max)
                      .OrderBy(a => a.PointsValue)
                      .ToList();
        }

        public async Task<SustainableActivity> CreateActivityAsync(SustainableActivity activity)
        {
            await _activityRepository.AddAsync(activity);
            return activity;
        }

        public async Task<bool> UpdateActivityAsync(SustainableActivity activity)
        {
            var existing = await _activityRepository.GetByIdAsync(activity.Id);
            if (existing == null) return false;

            existing.Title = activity.Title;
            existing.Description = activity.Description;
            existing.Category = activity.Category;
            existing.PointsValue = activity.PointsValue;
            existing.IsDaily = activity.IsDaily;
            existing.IsWeekly = activity.IsWeekly;
            existing.IsOneTime = activity.IsOneTime;

            await _activityRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteActivityAsync(int id)
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            if (activity == null) return false;

            await _activityRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> ActivityExists(int id)
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            return activity != null;
        }

        // Activity completion methods
        public async Task<List<ActivityCompletion>> GetCompletionsForActivityAsync(int activityId)
        {
            var allCompletions = await _activityCompletionRepository.GetAllAsync();
            return allCompletions.Where(ac => ac.ActivityId == activityId).ToList();
        }

        public async Task<List<ActivityCompletion>> GetCompletionsForUserAsync(int userId)
        {
            return await _activityCompletionRepository.GetByUserIdAsync(userId);
        }

        public async Task<ActivityCompletion?> GetCompletionByIdAsync(int completionId)
        {
            return await _activityCompletionRepository.GetByIdAsync(completionId);
        }

        public async Task AddCompletionAsync(ActivityCompletion completion)
        {
            await _activityCompletionRepository.AddAsync(completion);
        }

        public async Task UpdateCompletionAsync(ActivityCompletion completion)
        {
            await _activityCompletionRepository.UpdateAsync(completion);
        }

        public async Task DeleteCompletionAsync(int completionId)
        {
            await _activityCompletionRepository.DeleteAsync(completionId);
        }
    }
}