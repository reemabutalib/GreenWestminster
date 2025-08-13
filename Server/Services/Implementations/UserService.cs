using Server.Models;
using Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Repositories;
using Server.Repositories.Interfaces;
using Server.DTOs;
using Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Server.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IActivityCompletionRepository _activityCompletionRepository;
        private readonly IChallengeRepository _challengeRepository;
        private readonly ISustainableActivityRepository _activityRepository;
        private readonly AppDbContext _context;
        

        public UserService(
            IUserRepository userRepository,
            IActivityCompletionRepository activityCompletionRepository,
            IChallengeRepository challengeRepository,
            ISustainableActivityRepository activityRepository,
            AppDbContext context
            )
        {
            _userRepository = userRepository;
            _activityCompletionRepository = activityCompletionRepository;
            _challengeRepository = challengeRepository;
            _activityRepository = activityRepository;
            _context = context;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
            => await _userRepository.GetAllAsync();

        public async Task<User?> GetUserAsync(int id)
            => await _userRepository.GetByIdAsync(id);

        public async Task<User> CreateUserAsync(User user)
        {
            await _userRepository.AddAsync(user);
            return user;
        }

public async Task<IEnumerable<LeaderboardUserDto>> GetLeaderboardAsync(string timeFrame)
{
    DateTime? start = null;
    var now = DateTime.UtcNow;

    if (timeFrame == "month")
    {
        start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }
    else if (timeFrame == "week")
    {
        // ISO-ish week start: Monday
        var offset = ((int)now.DayOfWeek + 6) % 7; // Mon=0 ... Sun=6
        start = now.Date.AddDays(-offset);
    }

    // Sum approved points in the window
    var approved = _context.ActivityCompletions
        .Where(ac => ac.ReviewStatus == "Approved");

    if (start.HasValue)
        approved = approved.Where(ac => ac.CompletedAt >= start.Value);

    var pointsByUser = await approved
        .GroupBy(ac => ac.UserId)
        .Select(g => new { UserId = g.Key, Points = g.Sum(x => x.PointsEarned) })
        .ToListAsync();

            // 1. Get users from DB
            var users = await _context.Users
                .Where(u => u.Username != "sustainabilityteam")
                .ToListAsync();

            // 2. Join in memory
            var leaderboard = users
    .Select(u => new LeaderboardUserDto
    {
        Id = u.Id,
        Username = u.Username,
        Points = u.Points,
        CurrentStreak = u.CurrentStreak,
        Level = LevelingService.CalculateLevel(u.Points), // <-- Always calculate!
    })
    .OrderByDescending(u => u.Points)
    .ThenBy(u => u.Username)
    .ToList();

            return leaderboard;
        }



        public async Task<object?> GetUserStatsAsync(int id)
{
    var user = await _userRepository.GetByIdAsync(id);
    if (user == null) return null;

    var completions = await _context.ActivityCompletions
        .Include(ac => ac.Activity)
        .Where(ac => ac.UserId == id && ac.ReviewStatus == "Approved")
        .ToListAsync();

    // Adjust these categories to match your real values
    var treesPlanted = completions.Count(a => a.Activity.Category == "Tree Planting");
    var wasteRecycled = completions.Count(a => a.Activity.Category == "Recycling") * 0.5;
    var sustainableCommutes = completions.Count(a => a.Activity.Category == "Sustainable Transport");
    var waterSaved = completions.Count(a => a.Activity.Category == "Water Conservation") * 50;

    return new
    {
        TreesPlanted = treesPlanted,
        WasteRecycled = wasteRecycled,
        SustainableCommutes = sustainableCommutes,
        WaterSaved = waterSaved
    };
}


        public async Task<IEnumerable<object>> GetRecentActivitiesAsync(int id)
{
    var activities = await _activityCompletionRepository.GetByUserIdAsync(id);

    return activities
        .OrderByDescending(ac => ac.CompletedAt)
        .Take(10)
        .Select(ac => new
        {
            id = ac.Id,
            completedAt = ac.CompletedAt,
            pointsEarned = ac.PointsEarned,
            reviewStatus = ac.ReviewStatus,
            activity = new
            {
                id = ac.Activity.Id,
                title = ac.Activity.Title,
                description = ac.Activity.Description
            }
        })
        .ToList();
}


        public async Task<IEnumerable<object>> GetCompletedActivitiesAsync(int id, DateTime date)
{
    var activities = await _activityCompletionRepository.GetByUserIdAsync(id);
    return activities
        .Where(ac => ac.CompletedAt.Date == date.Date)
        .OrderByDescending(ac => ac.CompletedAt)
        .Select(ac => new
        {
            id = ac.Id,
            completedAt = ac.CompletedAt,
            pointsEarned = ac.PointsEarned,
            reviewStatus = ac.ReviewStatus,
            activity = new
            {
                id = ac.Activity.Id,
                title = ac.Activity.Title,
                description = ac.Activity.Description
            }
        })
        .ToList();
}


        public async Task<IEnumerable<object>> GetCompletedChallengesAsync(int id)
        {
            var completedChallenges = await _challengeRepository.GetCompletedChallengesByUserIdAsync(id);

            return completedChallenges.Select(uc => new
            {
                id = uc.Challenge.Id,
                title = uc.Challenge.Title,
                description = uc.Challenge.Description,
                category = uc.Challenge.Category,
                pointsReward = uc.Challenge.PointsReward,
                startDate = uc.Challenge.StartDate,
                endDate = uc.Challenge.EndDate,
                completedAt = uc.CompletedAt
            });
        }

        public async Task<object?> GetChallengeStatusAsync(int userId, int challengeId)
        {
            var userChallenge = await _challengeRepository.GetUserChallengeAsync(userId, challengeId);

            return new
            {
                hasJoined = userChallenge != null,
                isCompleted = userChallenge?.IsCompleted ?? false,
                progress = userChallenge?.Progress ?? 0
            };
        }

        public async Task<ActivityCompletion?> CompleteActivityAsync(int userId, int activityId)
        {
            // This logic may need to be moved to a domain service or the repository if you want to keep UserService thin.
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var activity = await _activityRepository.GetByIdAsync(activityId);
            if (activity == null) return null;

            if (activity.IsDaily)
            {
                var todayCompletion = (await _activityCompletionRepository.GetByUserIdAsync(userId))
                    .FirstOrDefault(ac => ac.ActivityId == activityId && ac.CompletedAt.Date == DateTime.UtcNow.Date);

                if (todayCompletion != null)
                    return null;
            }

            var completion = new ActivityCompletion
            {
                UserId = userId,
                ActivityId = activityId,
                CompletedAt = DateTime.UtcNow,
                PointsEarned = activity.PointsValue
            };

            user.Points += activity.PointsValue;

            TimeSpan? timeSinceLastActivity = user.LastActivityDate.HasValue
                ? (TimeSpan?)(DateTime.UtcNow - user.LastActivityDate.Value)
                : null;

            if (timeSinceLastActivity.HasValue && timeSinceLastActivity.Value.TotalDays <= 1)
            {
                user.CurrentStreak++;
                if (user.CurrentStreak > user.MaxStreak)
                    user.MaxStreak = user.CurrentStreak;
            }
            else
            {
                user.CurrentStreak = 1;
            }

            user.LastActivityDate = DateTime.UtcNow;

            await _activityCompletionRepository.AddAsync(completion);
            await _userRepository.UpdateAsync(user);

            return completion;
        }

        public async Task<object?> GetActivityStatsAsync(int userId)
{
    var activities = await _context.ActivityCompletions
        .Include(ac => ac.Activity)
        .Where(ac => ac.UserId == userId && ac.ReviewStatus == "Approved")
        .ToListAsync();

    var categoryCounts = activities
        .GroupBy(ac => ac.Activity.Category)
        .Select(g => new { name = g.Key, value = g.Count() })
        .ToList();

    var fourWeeksAgo = DateTime.UtcNow.AddDays(-28);

    // Mon=1 ... Sun=0 mapping you used; keep consistent
    var dayAbbreviations = new Dictionary<int, string>
    {
        { 1, "Mon" }, { 2, "Tue" }, { 3, "Wed" },
        { 4, "Thu" }, { 5, "Fri" }, { 6, "Sat" }, { 0, "Sun" }
    };

    var weeklyActivity = activities
        .Where(ac => ac.CompletedAt >= fourWeeksAgo)
        .GroupBy(ac => ((int)ac.CompletedAt.DayOfWeek))
        .Select(g => new { dayNum = g.Key, count = g.Count() })
        .ToList();

    var formattedWeeklyActivity = Enumerable.Range(0, 7)
        .Select(dayNum => new
        {
            day = dayAbbreviations[dayNum],
            count = weeklyActivity.FirstOrDefault(wa => wa.dayNum == dayNum)?.count ?? 0
        })
        .OrderBy(d => d.day == "Sun" ? 7 :
                      d.day == "Mon" ? 1 :
                      d.day == "Tue" ? 2 :
                      d.day == "Wed" ? 3 :
                      d.day == "Thu" ? 4 :
                      d.day == "Fri" ? 5 : 6)
        .ToList();

    return new
    {
        categoryCounts,
        weeklyActivity = formattedWeeklyActivity
    };
}


        public async Task<IEnumerable<object>> GetPointsHistoryAsync(int userId)
{
    var fourWeeksAgo = DateTime.UtcNow.AddDays(-28).Date;

    // Only approved completions contribute to points
    var approved = await _context.ActivityCompletions
        .Where(ac => ac.UserId == userId
            && ac.ReviewStatus == "Approved"
            && ac.CompletedAt >= fourWeeksAgo)
        .Select(ac => new { ac.CompletedAt, ac.PointsEarned })
        .ToListAsync();

    var weekStart = DateTime.UtcNow.Date.AddDays(-((int)DateTime.UtcNow.DayOfWeek + 6) % 7); // Monday this week
    var starts = new[]
    {
        weekStart.AddDays(-21),
        weekStart.AddDays(-14),
        weekStart.AddDays(-7),
        weekStart
    };

    var pointsHistory = starts.Select((s, i) => new
    {
        date = $"Week {i + 1}",
        points = approved.Where(ph => ph.CompletedAt.Date >= s && ph.CompletedAt.Date < s.AddDays(7))
                         .Sum(ph => ph.PointsEarned)
    }).ToList();

    return pointsHistory;
}


        public async Task<object> AddPointsAsync(int userId, int points)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return new { success = false, message = "User not found" };

            string oldLevel = user.Level;
            user.Points += points;
            user.Level = LevelingService.CalculateLevel(user.Points);

            await _userRepository.UpdateAsync(user);

            bool leveledUp = oldLevel != user.Level;

            if (leveledUp)
            {
                return new
                {
                    userId = user.Id,
                    username = user.Username,
                    points = user.Points,
                    newLevel = user.Level,
                    leveledUp = true,
                    pointsToNextLevel = LevelingService.PointsToNextLevel(user.Points)
                };
            }

            return new
            {
                userId = user.Id,
                username = user.Username,
                points = user.Points,
                level = user.Level,
                leveledUp = false,
                pointsToNextLevel = LevelingService.PointsToNextLevel(user.Points)
            };
        }

        public async Task<object?> GetLevelInfoAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            string currentLevel = LevelingService.CalculateLevel(user.Points);
            int pointsToNextLevel = LevelingService.PointsToNextLevel(user.Points);
            int totalPointsForCurrentLevel = LevelingService.GetPointsForLevel(currentLevel);
            var allLevels = LevelingService.GetAllLevels();
            int nextLevelIndex = allLevels.IndexOf(currentLevel) + 1;
            int totalPointsForNextLevel = nextLevelIndex < allLevels.Count
                ? LevelingService.GetPointsForLevel(allLevels[nextLevelIndex])
                : totalPointsForCurrentLevel;

            string nextLevel = nextLevelIndex < allLevels.Count ? allLevels[nextLevelIndex] : null;

            double progressPercentage = 0;
            if (pointsToNextLevel > 0)
            {
                int pointsInCurrentLevel = user.Points - totalPointsForCurrentLevel;
                int pointsRequiredForNextLevel = totalPointsForNextLevel - totalPointsForCurrentLevel;
                progressPercentage = Math.Round((double)pointsInCurrentLevel / pointsRequiredForNextLevel * 100, 1);
            }

            return new
            {
                userId = user.Id,
                username = user.Username,
                totalPoints = user.Points,
                currentLevel = currentLevel,
                nextLevel = nextLevel,
                pointsToNextLevel = pointsToNextLevel,
                progressPercentage = progressPercentage,
                levelThresholds = allLevels.Select(lvl => new { level = lvl, threshold = LevelingService.GetPointsForLevel(lvl) }).ToList()
            };
        }

        public async Task<bool> UpdateAvatarAsync(int userId, string avatarStyle)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;
            user.AvatarStyle = avatarStyle;
            await _userRepository.UpdateAsync(user);
            return true;
        }

    }
}