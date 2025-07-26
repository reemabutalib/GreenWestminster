using Server.Data;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // GET: api/users/leaderboard
    [HttpGet("leaderboard")]
    public async Task<ActionResult<IEnumerable<User>>> GetLeaderboard([FromQuery] string timeFrame = "all-time")
    {
        IQueryable<User> query = _context.Users;

        // Filter based on time frame
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
        // "all-time" doesn't need filtering

        return await query
            .OrderByDescending(u => u.Points)
            .Take(10)
            .ToListAsync();
    }

    // GET: api/users/5/stats
    [HttpGet("{id}/stats")]
    public async Task<ActionResult<object>> GetUserStats(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        // Calculate environmental stats from the user's completed activities
        var completedActivities = await _context.ActivityCompletions
            .Include(ac => ac.Activity)
            .Where(ac => ac.UserId == id)
            .ToListAsync();

        // You'll need to adjust these calculations based on your actual data model
        var treesPlanted = completedActivities.Count(a => a.Activity.Category == "Tree Planting");
        var wasteRecycled = completedActivities.Count(a => a.Activity.Category == "Recycling") * 0.5; // Assuming 0.5kg per recycling activity
        var sustainableCommutes = completedActivities.Count(a => a.Activity.Category == "Sustainable Transport");
        var waterSaved = completedActivities.Count(a => a.Activity.Category == "Water Conservation") * 50; // Assuming 50L per water-saving activity

        return new
        {
            TreesPlanted = treesPlanted,
            WasteRecycled = wasteRecycled,
            SustainableCommutes = sustainableCommutes,
            WaterSaved = waterSaved
        };
    }

    // GET: api/users/5/activities/recent
    [HttpGet("{id}/activities/recent")]
    public async Task<ActionResult<IEnumerable<object>>> GetRecentActivities(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var recentActivities = await _context.ActivityCompletions
          .Include(ac => ac.Activity)
          .Where(ac => ac.UserId == id)
          .OrderByDescending(ac => ac.CompletedAt) // Changed to CompletedAt
          .Take(10)
          .Select(ac => new
          {
              id = ac.Id,
              completedAt = ac.CompletedAt, // Changed to CompletedAt
              pointsEarned = ac.PointsEarned,
              activity = new
              {
                  id = ac.Activity.Id,
                  title = ac.Activity.Title,
                  description = ac.Activity.Description
              }
          })
          .ToListAsync();

        return recentActivities;
    }

    // GET: api/users/5/activities/completed
    [HttpGet("{id}/activities/completed")]
    public async Task<ActionResult<IEnumerable<object>>> GetCompletedActivities(int id, [FromQuery] DateTime date)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var completedActivities = await _context.ActivityCompletions
            .Include(ac => ac.Activity)
            .Where(ac => ac.UserId == id && ac.CompletedAt.Date == date.Date) // Changed to CompletedAt
            .Select(ac => new
            {
                id = ac.Id,
                completedAt = ac.CompletedAt, // Changed to CompletedAt
                pointsEarned = ac.PointsEarned,
                activity = new
                {
                    id = ac.Activity.Id,
                    title = ac.Activity.Title,
                    description = ac.Activity.Description
                }
            })
            .ToListAsync();

        return completedActivities;
    }

    // GET: api/users/5/challenges/completed
    [HttpGet("{id}/challenges/completed")]
    public async Task<ActionResult<IEnumerable<object>>> GetCompletedChallenges(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        // Assuming you have a UserChallenges table that tracks which challenges a user has completed
        var completedChallenges = await _context.UserChallenges
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
                completedAt = uc.CompletedAt // Changed from completedDate to match model
            })
            .ToListAsync();

        return completedChallenges;
    }

    // GET: api/users/5/challenges/1/status
    [HttpGet("{userId}/challenges/{challengeId}/status")]
    public async Task<ActionResult<object>> GetChallengeStatus(int userId, int challengeId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var challenge = await _context.Challenges.FindAsync(challengeId);

        if (challenge == null)
        {
            return NotFound("Challenge not found");
        }

        // Check if user has joined this challenge
        var userChallenge = await _context.UserChallenges
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);

        return new
        {
            hasJoined = userChallenge != null,
            isCompleted = userChallenge?.IsCompleted ?? false,
            progress = userChallenge?.Progress ?? 0
        };
    }

    // POST: api/users/5/completeActivity/3
    [HttpPost("{userId}/completeActivity/{activityId}")]
    public async Task<ActionResult<ActivityCompletion>> CompleteActivity(int userId, int activityId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("User not found");

        var activity = await _context.SustainableActivities.FindAsync(activityId);
        if (activity == null)
            return NotFound("Activity not found");

        // Check if user already completed this activity today (for daily activities)
        if (activity.IsDaily)
        {
            var todayCompletion = await _context.ActivityCompletions
                .Where(ac => ac.UserId == userId && ac.ActivityId == activityId &&
                        ac.CompletedAt.Date == DateTime.UtcNow.Date) // Changed to CompletedAt
                .FirstOrDefaultAsync();

            if (todayCompletion != null)
                return BadRequest("Activity already completed today");
        }

        var completion = new ActivityCompletion
        {
            UserId = userId,
            ActivityId = activityId,
            CompletedAt = DateTime.UtcNow, // Changed to CompletedAt
            PointsEarned = activity.PointsValue
        };

        // Update user stats
        user.Points += activity.PointsValue;

        // Update streak logic
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

        return CreatedAtAction(nameof(GetUser), new { id = userId }, completion);
    }

    // GET: api/users/{userId}/activity-stats
    [HttpGet("{userId}/activity-stats")]
    public async Task<ActionResult<object>> GetActivityStats(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        // Get category distribution
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

        // Get activity by day of week (for the past 4 weeks)
        var fourWeeksAgo = DateTime.UtcNow.AddDays(-28);

        // Create a lookup for day abbreviations
        var dayAbbreviations = new Dictionary<int, string>
    {
        { 1, "Mon" }, { 2, "Tue" }, { 3, "Wed" },
        { 4, "Thu" }, { 5, "Fri" }, { 6, "Sat" }, { 0, "Sun" }
    };

        // Query for activities by day of week
        var weeklyActivity = await _context.ActivityCompletions
            .Where(ac => ac.UserId == userId && ac.CompletedAt >= fourWeeksAgo)
            .GroupBy(ac => ((int)ac.CompletedAt.DayOfWeek)) // Sunday is 0, Monday is 1, etc.
            .Select(g => new
            {
                dayNum = g.Key,
                count = g.Count()
            })
            .ToListAsync();

        // Format and fill in any missing days
        var formattedWeeklyActivity = Enumerable.Range(0, 7)
            .Select(dayNum => new
            {
                day = dayAbbreviations[dayNum],
                count = weeklyActivity.FirstOrDefault(wa => wa.dayNum == dayNum)?.count ?? 0
            })
            .OrderBy(d => d.day == "Sun" ? 7 : // Make Sunday last
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

    // GET: api/users/{userId}/points-history
    [HttpGet("{userId}/points-history")]
    public async Task<ActionResult<IEnumerable<object>>> GetPointsHistory(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        // Get the start date (4 weeks ago)
        var fourWeeksAgo = DateTime.UtcNow.AddDays(-28).Date;

        // Create a week number lookup starting from 4 weeks ago
        var weekLookup = Enumerable.Range(0, 4)
            .ToDictionary(
                i => fourWeeksAgo.AddDays(i * 7).Date,
                i => $"Week {4 - i}"
            );

        // Aggregate points by week
        var pointsHistory = await _context.ActivityCompletions
            .Where(ac => ac.UserId == userId && ac.CompletedAt >= fourWeeksAgo)
            .GroupBy(ac => new
            {
                // Group by the start of each week
                WeekStart = ac.CompletedAt.Date.AddDays(-(int)ac.CompletedAt.DayOfWeek).Date
            })
            .Select(g => new
            {
                WeekStart = g.Key.WeekStart,
                Points = g.Sum(ac => ac.PointsEarned)
            })
            .ToListAsync();

        // Format the results to match the chart's expected structure,
        // filling in any missing weeks with zero points
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

    // Add a new method to update user points
    [HttpPost("{userId}/add-points")]
    public async Task<IActionResult> AddPoints(int userId, [FromBody] AddPointsRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        // Record the old level
        int oldLevel = user.Level;

        // Add points
        user.Points += request.Points;

        // Update the level based on new points total
        user.Level = LevelingService.CalculateLevel(user.Points);

        await _context.SaveChangesAsync();

        // Check if the user leveled up
        bool leveledUp = user.Level > oldLevel;

        // If the user leveled up, we might want to return additional info
        if (leveledUp)
        {
            return Ok(new
            {
                userId = user.Id,
                username = user.Username,
                points = user.Points,
                newLevel = user.Level,
                leveledUp = true,
                pointsToNextLevel = LevelingService.PointsToNextLevel(user.Points)
            });
        }

        return Ok(new
        {
            userId = user.Id,
            username = user.Username,
            points = user.Points,
            level = user.Level,
            leveledUp = false,
            pointsToNextLevel = LevelingService.PointsToNextLevel(user.Points)
        });
    }

[HttpGet("{userId}/level-info")]
public async Task<ActionResult<object>> GetLevelInfo(int userId)
{
    var user = await _context.Users.FindAsync(userId);
    
    if (user == null)
    {
        return NotFound("User not found");
    }
    
    int currentLevel = LevelingService.CalculateLevel(user.Points);
    int pointsToNextLevel = LevelingService.PointsToNextLevel(user.Points);
    int totalPointsForCurrentLevel = currentLevel > 1 ? LevelingService.GetPointsForLevel(currentLevel) : 0;
    int totalPointsForNextLevel = LevelingService.GetPointsForLevel(currentLevel + 1);
    
    // Calculate progress percentage to the next level
    double progressPercentage = 0;
    if (pointsToNextLevel > 0)
    {
        int pointsInCurrentLevel = user.Points - totalPointsForCurrentLevel;
        int pointsRequiredForNextLevel = totalPointsForNextLevel - totalPointsForCurrentLevel;
        progressPercentage = Math.Round((double)pointsInCurrentLevel / pointsRequiredForNextLevel * 100, 1);
    }
    
    return Ok(new
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
    });
}

    // Add this class for the request body
    public class AddPointsRequest
    {
        public int Points { get; set; }
    }

}