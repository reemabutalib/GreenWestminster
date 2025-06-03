using Server.Data;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                completedDate = uc.CompletedDate
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
                       ac.CompletedAt.Date == DateTime.UtcNow.Date)
                .FirstOrDefaultAsync();
                
            if (todayCompletion != null)
                return BadRequest("Activity already completed today");
        }
        
        var completion = new ActivityCompletion
        {
            UserId = userId,
            ActivityId = activityId,
            CompletedAt = DateTime.UtcNow,
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
}