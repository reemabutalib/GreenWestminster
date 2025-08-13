using Server.Models;
using Server.Services.Interfaces;
using Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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

        // ─────────────────────────────────────────────────────────────
        // ACTIVITY CRUD
        // ─────────────────────────────────────────────────────────────
        public async Task<List<SustainableActivity>> GetAllActivitiesAsync()
            => await _activityRepository.GetAllAsync();

        public async Task<SustainableActivity?> GetActivityByIdAsync(int id)
            => await _activityRepository.GetByIdAsync(id);

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
            => await _activityRepository.GetByIdAsync(id) != null;

        // ─────────────────────────────────────────────────────────────
        // COMPLETIONS
        // ─────────────────────────────────────────────────────────────

        // All completions (Approved, Pending, Rejected)
        public async Task<List<ActivityCompletion>> GetUserCompletionsAsync(int userId)
        {
            var completions = await _activityCompletionRepository.GetByUserIdAsync(userId);
            return completions
                .OrderByDescending(ac => ac.CompletedAt)
                .ToList();
        }

        // Only Pending Review completions
        public async Task<List<ActivityCompletion>> GetPendingCompletionsForUserAsync(int userId)
        {
            var completions = await _activityCompletionRepository.GetByUserIdAsync(userId);
            return completions
                .Where(ac => !string.IsNullOrEmpty(ac.ReviewStatus) &&
                             ac.ReviewStatus.Equals("Pending Review", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(ac => ac.CompletedAt)
                .ToList();
        }

        // Resubmit a rejected activity
        public async Task<(bool Success, string Message, ActivityCompletion? Completion)>
            ResubmitActivityAsync(int completionId, int userId, string? notes, IFormFile? image)
        {
            var completion = await _activityCompletionRepository.GetByIdAsync(completionId);

            if (completion == null)
                return (false, $"Completion {completionId} not found", null);

            if (completion.UserId != userId)
                return (false, "You can only resubmit your own activities", null);

            if (!string.Equals(completion.ReviewStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
                return (false, "Only rejected activities can be resubmitted", null);

            // Update notes
            completion.Notes = notes;
            completion.ReviewStatus = "Pending Review";
            completion.AdminNotes = null; // Clear admin notes on resubmission
            completion.CompletedAt = DateTime.UtcNow;

            // If new image is provided
            if (image != null && image.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                var fileExtension = Path.GetExtension(image.FileName);
                var fileName = $"activity_{completion.ActivityId}_user_{completion.UserId}_{timestamp}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                completion.ImagePath = fileName;
            }

            await _activityCompletionRepository.UpdateAsync(completion);
            return (true, "Resubmitted successfully", completion);
        }
    }
}
