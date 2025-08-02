using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Data;
using Server.Models;
using Server.Services.Interfaces;
using Server.DTOs;

namespace Server.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(AppDbContext context, ILogger<AdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<object> GetUserStatsAsync()
        {
            _logger.LogInformation("AdminService: Fetching user engagement statistics");

            int totalUsers = await _context.Users.CountAsync();
            int activeUsersLast7Days = await _context.Users.CountAsync(u => u.LastActivityDate >= DateTime.UtcNow.AddDays(-7));
            int activeUsersLast30Days = await _context.Users.CountAsync(u => u.LastActivityDate >= DateTime.UtcNow.AddDays(-30));

            var topUsers = await _context.Users
                .OrderByDescending(u => u.Points)
                .Take(10)
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    points = u.Points,
                    currentStreak = u.CurrentStreak,
                    maxStreak = u.MaxStreak,
                    joinDate = u.JoinDate
                })
                .ToListAsync();

            int totalActivities = await _context.SustainableActivities.CountAsync();
            int totalCompletions = await _context.ActivityCompletions.CountAsync();

            var popularActivities = await _context.ActivityCompletions
                .GroupBy(ac => ac.ActivityId)
                .Select(g => new
                {
                    activityId = g.Key,
                    completionCount = g.Count()
                })
                .OrderByDescending(a => a.completionCount)
                .Take(5)
                .ToListAsync();

            var activityIds = popularActivities.Select(a => a.activityId).ToList();
            var activityDetails = await _context.SustainableActivities
                .Where(a => activityIds.Contains(a.Id))
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    category = a.Category
                })
                .ToDictionaryAsync(a => a.id, a => new { a.title, a.category });

            var popularActivitiesWithDetails = popularActivities
                .Select(a => new
                {
                    activityId = a.activityId,
                    title = activityDetails.ContainsKey(a.activityId) ? activityDetails[a.activityId].title : "Unknown Activity",
                    category = activityDetails.ContainsKey(a.activityId) ? activityDetails[a.activityId].category : "Unknown",
                    completionCount = a.completionCount
                })
                .ToList();

            int totalEvents = await _context.SustainabilityEvents.CountAsync();
            int upcomingEvents = await _context.SustainabilityEvents.CountAsync(e => e.StartDate > DateTime.UtcNow);

            var pointsDistributionQuery = await _context.Users
                .GroupBy(u => u.Points / 100)
                .Select(g => new
                {
                    pointRange = g.Key,
                    userCount = g.Count()
                })
                .OrderBy(g => g.pointRange)
                .ToListAsync();

            var pointsDistribution = pointsDistributionQuery
                .Select(g => new
                {
                    pointRange = $"{g.pointRange * 100}-{(g.pointRange + 1) * 100 - 1}",
                    userCount = g.userCount
                })
                .ToList();

            return new
            {
                totalUsers,
                activeUsersLast7Days,
                activeUsersLast30Days,
                topUsers,
                totalActivities,
                totalCompletions,
                popularActivitiesWithDetails,
                totalEvents,
                upcomingEvents,
                pointsDistribution
            };
        }

        public async Task<object> GetActivityCompletionsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            _logger.LogInformation("AdminService: Fetching activity completions");

            var query = _context.ActivityCompletions.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(ac => ac.CompletedAt >= startDate.Value); // Changed property name
            }

            if (endDate.HasValue)
            {
                query = query.Where(ac => ac.CompletedAt <= endDate.Value); // Changed property name
            }

            var completions = await query
                .GroupBy(ac => ac.ActivityId)
                .Select(g => new
                {
                    activityId = g.Key,
                    completionCount = g.Count()
                })
                .ToListAsync();

            return new
            {
                totalCompletions = completions.Sum(c => c.completionCount),
                activityCompletions = completions
            };
        }
    }
}