using Server.Data;
using Server.Models;
using Microsoft.EntityFrameworkCore;
using Server.Services.Interfaces;
using Server.DTOs;

namespace Server.Services.Implementations
{
    public class ActivitiesService : IActivitiesService
    {
        private readonly AppDbContext _context;

        public ActivitiesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SustainableActivity>> GetAllActivitiesAsync()
        {
            return await _context.SustainableActivities.AsNoTracking().ToListAsync();
        }

        public async Task<SustainableActivity?> GetActivityByIdAsync(int id)
        {
            return await _context.SustainableActivities.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<SustainableActivity>> GetActivitiesByCategoryAsync(string category)
        {
            return await _context.SustainableActivities
                .AsNoTracking()
                .Where(a => EF.Functions.ILike(a.Category, $"%{category}%"))
                .ToListAsync();
        }

        public async Task<List<SustainableActivity>> GetDailyActivitiesAsync()
        {
            return await _context.SustainableActivities
                .AsNoTracking()
                .Where(a => a.IsDaily)
                .ToListAsync();
        }

        public async Task<List<SustainableActivity>> GetWeeklyActivitiesAsync()
        {
            return await _context.SustainableActivities
                .AsNoTracking()
                .Where(a => a.IsWeekly)
                .ToListAsync();
        }

        public async Task<List<SustainableActivity>> GetActivitiesByPointsRangeAsync(int min, int max)
        {
            return await _context.SustainableActivities
                .AsNoTracking()
                .Where(a => a.PointsValue >= min && a.PointsValue <= max)
                .OrderBy(a => a.PointsValue)
                .ToListAsync();
        }

        public async Task<SustainableActivity> CreateActivityAsync(SustainableActivity activity)
        {
            _context.SustainableActivities.Add(activity);
            await _context.SaveChangesAsync();
            return activity;
        }

        public async Task<bool> UpdateActivityAsync(SustainableActivity activity)
        {
            var existing = await _context.SustainableActivities.FindAsync(activity.Id);
            if (existing == null) return false;

            existing.Title = activity.Title;
            existing.Description = activity.Description;
            existing.Category = activity.Category;
            existing.PointsValue = activity.PointsValue;
            existing.IsDaily = activity.IsDaily;
            existing.IsWeekly = activity.IsWeekly;
            existing.IsOneTime = activity.IsOneTime;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteActivityAsync(int id)
        {
            var activity = await _context.SustainableActivities.FindAsync(id);
            if (activity == null) return false;

            _context.SustainableActivities.Remove(activity);
            await _context.SaveChangesAsync();
            return true;
        }

        public bool ActivityExists(int id)
        {
            return _context.SustainableActivities.Any(e => e.Id == id);
        }
    }
}