using Server.Data;
using Server.Models;
using Microsoft.EntityFrameworkCore;
using Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.DTOs;

namespace Server.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetLeaderboardAsync(string timeFrame)
        {
            IQueryable<User> query = _context.Users;

            if (timeFrame == "week")
            {
                DateTime oneWeekAgo = DateTime.UtcNow.AddDays(-7);
                query = query.Where(u => u.LastActivityDate >= oneWeekAgo);
            }
            else if (timeFrame == "month")
            {
                DateTime oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
                query = query.Where(u => u.LastActivityDate >= oneMonthAgo);
            }

            return await query
                .OrderByDescending(u => u.Points)
                .Take(10)
                .ToListAsync();
        }

        public async Task<object?> GetUserStatsAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            var completedActivities = await _context.ActivityCompletions
                .Include(ac => ac.Activity)
                .Where(ac => ac.UserId == id)
                .ToListAsync();

            var treesPlanted = completedActivities.Count(a => a.Activity.Category == "Tree Planting");
            var wasteRecycled = completedActivities.Count(a => a.Activity.Category == "Recycling") * 0.5;
            var sustainableCommutes = completedActivities.Count(a => a.Activity.Category == "Sustainable Transport");
            var waterSaved = completedActivities.Count(a => a.Activity.Category == "Water Conservation") * 50;

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
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Enumerable.Empty<object>();

            return await _context.ActivityCompletions
                .Include(ac => ac.Activity)
                .Where(ac => ac.UserId == id)
                .OrderByDescending(ac => ac.CompletedAt)
                .Take(10)
                .Select(ac => new
                {
                    id = ac.Id,
                    completedAt = ac.CompletedAt,
                    pointsEarned = ac.PointsEarned,
                    activity = new
                    {
                        id = ac.Activity.Id,
                        title = ac.Activity.Title,
                        description = ac.Activity.Description
                    }
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetCompletedActivitiesAsync(int id, DateTime date)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Enumerable.Empty<object>();

            return await _context.ActivityCompletions
                .Include(ac => ac.Activity)
                .Where(ac => ac.UserId == id && ac.CompletedAt.Date == date.Date)
                .Select(ac => new
                {
                    id = ac.Id,
                    completedAt = ac.CompletedAt,
                    pointsEarned = ac.PointsEarned,
                    activity = new
                    {
                        id = ac.Activity.Id,
                        title = ac.Activity.Title,
                        description = ac.Activity.Description
                    }
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetCompletedChallengesAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Enumerable.Empty<object>();

            return await _context.UserChallenges
                .Include(uc => uc.Challenge)
                .Where(uc => uc.UserId == id && uc.IsCompleted)
                .Select(uc => new
                {
                    id = uc.Challenge.Id,
                    title = uc.Challenge.Title,
                    description = uc.Challenge.Description,
                    category = uc.Challenge.Category,
                    pointsReward = uc.Challenge.PointsReward,
                    startDate = uc.Challenge.StartDate,
                    endDate = uc.Challenge.EndDate,
                    completedAt = uc.CompletedAt
                })
                .ToListAsync();
        }

        public async Task<object?> GetChallengeStatusAsync(int userId, int challengeId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var challenge = await _context.Challenges.FindAsync(challengeId);
            if (challenge == null) return null;

            var userChallenge = await _context.UserChallenges
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);

            return new
            {
                hasJoined = userChallenge != null,
                isCompleted = userChallenge?.IsCompleted ?? false,
                progress = userChallenge?.Progress ?? 0
            };
        }

        public async Task<ActivityCompletion?> CompleteActivityAsync(int userId, int activityId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var activity = await _context.SustainableActivities.FindAsync(activityId);
            if (activity == null) return null;

            if (activity.IsDaily)
            {
                var todayCompletion = await _context.ActivityCompletions
                    .Where(ac => ac.UserId == userId && ac.ActivityId == activityId &&
                            ac.CompletedAt.Date == DateTime.UtcNow.Date)
                    .FirstOrDefaultAsync();

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

            TimeSpan timeSinceLastActivity = DateTime.UtcNow - user.LastActivityDate;
            if (timeSinceLastActivity.TotalDays <= 1)
            {
                user.CurrentStreak++;
                if (user.CurrentStreak > user.MaxStreak)
                    user.MaxStreak = user.CurrentStreak;
            }
            else if (timeSinceLastActivity.TotalDays > 1)
            {
                user.CurrentStreak = 1;
            }

            user.LastActivityDate = DateTime.UtcNow;

            _context.ActivityCompletions.Add(completion);
            await _context.SaveChangesAsync();

            return completion;
        }

        public async Task<object?> GetActivityStatsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var categoryCounts = await _context.ActivityCompletions
                .Include(ac => ac.Activity)
                .Where(ac => ac.UserId == userId)
                .GroupBy(ac => ac.Activity.Category)
                .Select(g => new
                {
                    name = g.Key,
                    value = g.Count()
                })
                .ToListAsync();

            var fourWeeksAgo = DateTime.UtcNow.AddDays(-28);

            var dayAbbreviations = new Dictionary<int, string>
            {
                { 1, "Mon" }, { 2, "Tue" }, { 3, "Wed" },
                { 4, "Thu" }, { 5, "Fri" }, { 6, "Sat" }, { 0, "Sun" }
            };

            var weeklyActivity = await _context.ActivityCompletions
                .Where(ac => ac.UserId == userId && ac.CompletedAt >= fourWeeksAgo)
                .GroupBy(ac => ((int)ac.CompletedAt.DayOfWeek))
                .Select(g => new
                {
                    dayNum = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

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
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Enumerable.Empty<object>();

            var fourWeeksAgo = DateTime.UtcNow.AddDays(-28).Date;

            var weekLookup = Enumerable.Range(0, 4)
                .ToDictionary(
                    i => fourWeeksAgo.AddDays(i * 7).Date,
                    i => $"Week {4 - i}"
                );

            var pointsHistory = await _context.ActivityCompletions
                .Where(ac => ac.UserId == userId && ac.CompletedAt >= fourWeeksAgo)
                .GroupBy(ac => new
                {
                    WeekStart = ac.CompletedAt.Date.AddDays(-(int)ac.CompletedAt.DayOfWeek).Date
                })
                .Select(g => new
                {
                    WeekStart = g.Key.WeekStart,
                    Points = g.Sum(ac => ac.PointsEarned)
                })
                .ToListAsync();

            var formattedPointsHistory = weekLookup.Keys
                .OrderBy(date => date)
                .Select(weekStart => new
                {
                    date = weekLookup[weekStart],
                    points = pointsHistory.FirstOrDefault(ph =>
                        ph.WeekStart == weekStart ||
                        (ph.WeekStart > weekStart && ph.WeekStart < weekStart.AddDays(7)))?.Points ?? 0
                })
                .ToList();

            return formattedPointsHistory;
        }

        public async Task<object> AddPointsAsync(int userId, int points)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new { success = false, message = "User not found" };

            int oldLevel = user.Level;
            user.Points += points;
            user.Level = LevelingService.CalculateLevel(user.Points);

            await _context.SaveChangesAsync();

            bool leveledUp = user.Level > oldLevel;

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
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            int currentLevel = LevelingService.CalculateLevel(user.Points);
            int pointsToNextLevel = LevelingService.PointsToNextLevel(user.Points);
            int totalPointsForCurrentLevel = currentLevel > 1 ? LevelingService.GetPointsForLevel(currentLevel) : 0;
            int totalPointsForNextLevel = LevelingService.GetPointsForLevel(currentLevel + 1);

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
                pointsToNextLevel = pointsToNextLevel,
                progressPercentage = progressPercentage,
                levelThresholds = new[] {
                    new { level = 1, threshold = 0 },
                    new { level = 2, threshold = 100 },
                    new { level = 3, threshold = 250 },
                    new { level = 4, threshold = 500 },
                    new { level = 5, threshold = 1000 }
                }
            };
        }
    }
}