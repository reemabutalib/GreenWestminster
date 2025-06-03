using Server.Data;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<IEnumerable<User>>> GetLeaderboard()
    {
        return await _context.Users
            .OrderByDescending(u => u.Points)
            .Take(10)
            .ToListAsync();
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