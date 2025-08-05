using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Models;
using Server.Repositories.Interfaces;
using Server.Services.Interfaces;
using Server.Repositories;
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

            var users = await _userRepository.GetAllAsync();
            int totalUsers = users.Count;
            int activeUsersLast7Days = users.Count(u => u.LastActivityDate >= DateTime.UtcNow.AddDays(-7));
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

            var activities = await _activityCompletionRepository.GetAllAsync();
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

            // Get activity details for popular activities
            var activityIds = activityGroups.Select(a => a.activityId).ToList();
            var allActivities = await Task.FromResult(activities.Select(ac => ac.Activity).Distinct().ToList());
            var activityDetails = allActivities
                .Where(a => activityIds.Contains(a.Id))
                .ToDictionary(a => a.Id, a => new { a.Title, a.Category });

            var popularActivitiesWithDetails = activityGroups
                .Select(a => new
                {
                    activityId = a.activityId,
                    title = activityDetails.ContainsKey(a.activityId) ? activityDetails[a.activityId].Title : "Unknown Activity",
                    category = activityDetails.ContainsKey(a.activityId) ? activityDetails[a.activityId].Category : "Unknown",
                    completionCount = a.completionCount
                })
                .ToList();

            var sustainableActivities = allActivities;
            int totalActivities = sustainableActivities.Count;

            var events = await _eventRepository.GetAllAsync();
            int totalEvents = events.Count;
            int upcomingEvents = events.Count(e => e.StartDate > DateTime.UtcNow);

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

            var completions = await _activityCompletionRepository.GetAllAsync();

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