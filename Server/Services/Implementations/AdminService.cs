using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Models;
using Server.Repositories.Interfaces;
using Server.Services.Interfaces;
using System.Collections.Generic;

namespace Server.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IActivityCompletionRepository _activityCompletionRepository;
        private readonly IChallengeRepository _challengeRepository;
        private readonly ISustainableEventRepository _eventRepository;
        private readonly ILogger<AdminService> _logger;

        // Central place to exclude usernames from admin views/stats
        private static readonly HashSet<string> EXCLUDED_USERNAMES = new(StringComparer.OrdinalIgnoreCase)
        {
            "sustainabilityteam"
        };

        public AdminService(
            IUserRepository userRepository,
            IActivityCompletionRepository activityCompletionRepository,
            IChallengeRepository challengeRepository,
            ISustainableEventRepository eventRepository,
            ILogger<AdminService> logger)
        {
            _userRepository = userRepository;
            _activityCompletionRepository = activityCompletionRepository;
            _challengeRepository = challengeRepository;
            _eventRepository = eventRepository;
            _logger = logger;
        }

        public async Task<object> GetUserStatsAsync()
        {
            _logger.LogInformation("AdminService: Fetching user engagement statistics");

            // Users (exclude sustainabilityteam)
            var usersAll = await _userRepository.GetAllAsync();
            var users = usersAll.Where(u => !EXCLUDED_USERNAMES.Contains(u.Username)).ToList();

            int totalUsers = users.Count;
            int activeUsersLast7Days  = users.Count(u => u.LastActivityDate >= DateTime.UtcNow.AddDays(-7));
            int activeUsersLast30Days = users.Count(u => u.LastActivityDate >= DateTime.UtcNow.AddDays(-30));

            var topUsers = users
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
                .ToList();

            // Activity completions (exclude those created by sustainabilityteam)
            var allCompletions = await _activityCompletionRepository.GetAllAsync();
            var activities = allCompletions
                .Where(ac => ac.User != null && !EXCLUDED_USERNAMES.Contains(ac.User.Username))
                .ToList();

            int totalCompletions = activities.Count;

            var activityGroups = activities
                .GroupBy(ac => ac.ActivityId)
                .Select(g => new
                {
                    activityId = g.Key,
                    completionCount = g.Count()
                })
                .OrderByDescending(a => a.completionCount)
                .Take(5)
                .ToList();

            // Look up activity details for popular activities
            var activityIds = activityGroups.Select(a => a.activityId).ToList();
            var allActivities = activities
                .Where(ac => ac.Activity != null)
                .Select(ac => ac.Activity)
                .Distinct()
                .ToList();

            var activityDetails = allActivities
                .Where(a => a != null)
                .ToDictionary(a => a.Id, a => new { a.Title, a.Category });

            var popularActivitiesWithDetails = activityGroups
                .Select(a => new
                {
                    activityId = a.activityId,
                    title = activityDetails.TryGetValue(a.activityId, out var info) ? info.Title : "Unknown Activity",
                    category = activityDetails.TryGetValue(a.activityId, out var info2) ? info2.Category : "Unknown",
                    completionCount = a.completionCount
                })
                .ToList();

            int totalActivities = allActivities.Count;

            // Events
            var events = await _eventRepository.GetAllAsync();
            int totalEvents = events.Count;
            int upcomingEvents = events.Count(e => e.StartDate > DateTime.UtcNow);

            // Points distribution (based on filtered users)
            var pointsDistribution = users
                .GroupBy(u => u.Points / 100)
                .Select(g => new
                {
                    pointRange = $"{g.Key * 100}-{(g.Key + 1) * 100 - 1}",
                    userCount = g.Count()
                })
                .OrderBy(g => g.pointRange)
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

            var all = await _activityCompletionRepository.GetAllAsync();

            // Exclude sustainabilityteam here too
            var completions = all
                .Where(ac => ac.User != null && !EXCLUDED_USERNAMES.Contains(ac.User.Username))
                .ToList();

            if (startDate.HasValue)
                completions = completions.Where(ac => ac.CompletedAt >= startDate.Value).ToList();

            if (endDate.HasValue)
                completions = completions.Where(ac => ac.CompletedAt <= endDate.Value).ToList();

            var grouped = completions
                .GroupBy(ac => ac.ActivityId)
                .Select(g => new
                {
                    activityId = g.Key,
                    completionCount = g.Count()
                })
                .ToList();

            return new
            {
                totalCompletions = grouped.Sum(c => c.completionCount),
                activityCompletions = grouped
            };
        }
    }
}
