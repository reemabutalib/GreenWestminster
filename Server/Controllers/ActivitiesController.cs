using Server.Data;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Npgsql;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ActivitiesController> _logger;

    // Constructor with debugging options enabled
    public ActivitiesController(AppDbContext context, ILogger<ActivitiesController> logger)
    {
        _context = context;
        _logger = logger;
        _context.Database.SetCommandTimeout(60); // Increase timeout for debugging
    }

    // GET: api/activities
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetActivities()
    {
        try
        {
            _logger.LogInformation("Fetching all sustainable activities");
            
            var activities = await _context.SustainableActivities
                .AsNoTracking()
                .Select(a => new 
                {
                    id = a.Id,
                    title = a.Title,
                    description = a.Description,
                    category = a.Category,
                    pointsValue = a.PointsValue,
                    isDaily = a.IsDaily
                })
                .ToListAsync();
                
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching activities");
            return StatusCode(500, new { message = "An error occurred while retrieving activities", error = ex.Message });
        }
    }

    // GET: api/activities/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetActivity(int id)
    {
        try
        {
            _logger.LogInformation("Fetching activity with ID: {Id}", id);
            
            var activity = await _context.SustainableActivities
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new 
                {
                    id = a.Id,
                    title = a.Title,
                    description = a.Description,
                    category = a.Category,
                    pointsValue = a.PointsValue,
                    isDaily = a.IsDaily
                })
                .FirstOrDefaultAsync();

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID: {Id} not found", id);
                return NotFound(new { message = $"Activity with ID: {id} not found" });
            }

            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching activity with ID: {Id}", id);
            return StatusCode(500, new { message = $"An error occurred while retrieving activity with ID: {id}", error = ex.Message });
        }
    }

    // POST: api/activities
    [HttpPost]
    [Authorize] 
    public async Task<ActionResult<object>> CreateActivity(SustainableActivity activity)
    {
        try
        {
            _logger.LogInformation("Creating new activity: {Title}", activity.Title);
            
            if (activity == null)
            {
                return BadRequest(new { message = "Activity data is required" });
            }
            
            _context.SustainableActivities.Add(activity);
            await _context.SaveChangesAsync();

            var createdActivity = new
            {
                id = activity.Id,
                title = activity.Title,
                description = activity.Description,
                category = activity.Category,
                pointsValue = activity.PointsValue,
                isDaily = activity.IsDaily
            };

            return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, createdActivity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating activity");
            return StatusCode(500, new { message = "An error occurred while creating the activity", error = ex.Message });
        }
    }

    // PUT: api/activities/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateActivity(int id, SustainableActivity activity)
    {
        try
        {
            if (id != activity.Id)
            {
                return BadRequest(new { message = "Activity ID mismatch" });
            }

            _logger.LogInformation("Updating activity with ID: {Id}", id);
            
            var existingActivity = await _context.SustainableActivities.FindAsync(id);
            if (existingActivity == null)
            {
                return NotFound(new { message = $"Activity with ID: {id} not found" });
            }
            
            // Update only the scalar properties
            existingActivity.Title = activity.Title;
            existingActivity.Description = activity.Description;
            existingActivity.Category = activity.Category;
            existingActivity.PointsValue = activity.PointsValue;
            existingActivity.IsDaily = activity.IsDaily;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating activity with ID: {Id}", id);
                return StatusCode(500, new { message = $"Concurrency error while updating activity", error = ex.Message });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating activity with ID: {Id}", id);
            return StatusCode(500, new { message = $"An error occurred while updating activity with ID: {id}", error = ex.Message });
        }
    }

    // DELETE: api/activities/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        try
        {
            _logger.LogInformation("Deleting activity with ID: {Id}", id);
            
            var activity = await _context.SustainableActivities.FindAsync(id);
            if (activity == null)
            {
                _logger.LogWarning("Activity with ID: {Id} not found during delete operation", id);
                return NotFound(new { message = $"Activity with ID: {id} not found" });
            }

            _context.SustainableActivities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting activity with ID: {Id}", id);
            return StatusCode(500, new { message = $"An error occurred while deleting activity with ID: {id}", error = ex.Message });
        }
    }

    // GET: api/activities/category/waste-reduction
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<object>>> GetActivitiesByCategory(string category)
    {
        try
        {
            _logger.LogInformation("Fetching activities by category: {Category}", category);
            
            var activities = await _context.SustainableActivities
                .AsNoTracking()
                .Where(a => EF.Functions.ILike(a.Category, $"%{category}%"))
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    description = a.Description,
                    category = a.Category,
                    pointsValue = a.PointsValue,
                    isDaily = a.IsDaily
                })
                .ToListAsync();

            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching activities by category: {Category}", category);
            return StatusCode(500, new { message = $"An error occurred while retrieving activities for category: {category}", error = ex.Message });
        }
    }

    // GET: api/activities/daily
    [HttpGet("daily")]
    public async Task<ActionResult<IEnumerable<object>>> GetDailyActivities()
    {
        try
        {
            _logger.LogInformation("Fetching daily activities");
            
            var activities = await _context.SustainableActivities
                .AsNoTracking()
                .Where(a => a.IsDaily)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    description = a.Description,
                    category = a.Category,
                    pointsValue = a.PointsValue,
                    isDaily = a.IsDaily
                })
                .ToListAsync();

            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching daily activities");
            return StatusCode(500, new { message = "An error occurred while retrieving daily activities", error = ex.Message });
        }
    }

    // GET: api/activities/points-range?min=10&max=50
    [HttpGet("points-range")]
    public async Task<ActionResult<IEnumerable<object>>> GetActivitiesByPointsRange([FromQuery] int min = 0, [FromQuery] int max = int.MaxValue)
    {
        try
        {
            _logger.LogInformation("Fetching activities in points range: {Min} to {Max}", min, max);
            
            var activities = await _context.SustainableActivities
                .AsNoTracking()
                .Where(a => a.PointsValue >= min && a.PointsValue <= max)
                .OrderBy(a => a.PointsValue)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    description = a.Description,
                    category = a.Category,
                    pointsValue = a.PointsValue,
                    isDaily = a.IsDaily
                })
                .ToListAsync();

            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching activities by points range: {Min} to {Max}", min, max);
            return StatusCode(500, new { message = $"An error occurred while retrieving activities in points range: {min} to {max}", error = ex.Message });
        }
    }

// POST: api/activities/{id}/complete
[HttpPost("{id}/complete")]
[Authorize]
public async Task<IActionResult> CompleteActivity(int id, [FromBody] ActivityCompletionDto completionData)
{
    try
    {
        _logger.LogInformation("Marking activity {Id} as completed for user {UserId}", id, completionData.UserId);
        
        // IMPORTANT: Use a simple query that doesn't attempt to join with Challenges table
        // We use AsNoTracking and select only the fields we need to avoid any problematic navigation properties
        var activityQuery = "SELECT id, pointsvalue FROM sustainableactivities WHERE id = @id";
        var activityParam = new NpgsqlParameter("id", id);
        
        var activity = await _context.SustainableActivities
            .FromSqlRaw(activityQuery, activityParam)
            .Select(a => new { a.Id, a.PointsValue })
            .AsNoTracking()
            .FirstOrDefaultAsync();
            
        if (activity == null)
        {
            _logger.LogWarning("Activity {Id} not found during completion attempt", id);
            return NotFound(new { message = $"Activity with ID {id} not found" });
        }

        // Get the user with a direct query to avoid any navigation properties
        var userQuery = "SELECT id, points, currentstreak, maxstreak FROM users WHERE id = @userId";
        var userParam = new NpgsqlParameter("userId", completionData.UserId);
        
        var user = await _context.Users
            .FromSqlRaw(userQuery, userParam)
            .Select(u => new { u.Id, u.Points, u.CurrentStreak, u.MaxStreak })
            .AsNoTracking()
            .FirstOrDefaultAsync();
            
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found during activity completion", completionData.UserId);
            return NotFound(new { message = $"User with ID {completionData.UserId} not found" });
        }

        // Check if already completed today with direct SQL
        var today = DateTime.UtcNow.Date;
        var checkCompletedQuery = @"
            SELECT COUNT(*) FROM activitycompletions 
            WHERE userid = @userId AND activityid = @activityId AND 
                  completedat >= @startDate AND completedat < @endDate";
                  
        var checkParams = new[]
        {
            new NpgsqlParameter("userId", completionData.UserId),
            new NpgsqlParameter("activityId", id),
            new NpgsqlParameter("startDate", today),
            new NpgsqlParameter("endDate", today.AddDays(1))
        };
        
        var completionCount = await _context.Database
            .ExecuteSqlRawAsync(checkCompletedQuery, checkParams);
            
        if (completionCount > 0)
        {
            _logger.LogInformation("Activity {Id} already completed by user {UserId} today", id, completionData.UserId);
            return BadRequest(new { message = "Activity already completed today" });
        }

        // Insert completion with direct SQL
        var completedAt = completionData.CompletedAt != null 
            ? DateTime.SpecifyKind(completionData.CompletedAt.Value, DateTimeKind.Utc) 
            : DateTime.UtcNow;
            
        var insertSql = @"
            INSERT INTO activitycompletions (userid, activityid, completedat) 
            VALUES (@userId, @activityId, @completedAt)";
            
        var insertParams = new[]
        {
            new NpgsqlParameter("userId", completionData.UserId),
            new NpgsqlParameter("activityId", id),
            new NpgsqlParameter("completedAt", completedAt)
        };
        
        await _context.Database.ExecuteSqlRawAsync(insertSql, insertParams);

        // Check for yesterday's activity with direct SQL
        var yesterday = today.AddDays(-1);
        var checkYesterdayQuery = @"
            SELECT COUNT(*) FROM activitycompletions 
            WHERE userid = @userId AND 
                  completedat >= @startDate AND completedat < @endDate";
                  
        var yesterdayParams = new[]
        {
            new NpgsqlParameter("userId", completionData.UserId),
            new NpgsqlParameter("startDate", yesterday),
            new NpgsqlParameter("endDate", today)
        };
        
        var yesterdayCount = await _context.Database
            .ExecuteSqlRawAsync(checkYesterdayQuery, yesterdayParams);
        
        // Update user stats with direct SQL
        int newStreak = 1;
        if (yesterdayCount > 0)
        {
            newStreak = user.CurrentStreak + 1;
        }
        
        int newMaxStreak = Math.Max(newStreak, user.MaxStreak);
        int newPoints = user.Points + activity.PointsValue;
        
        var updateUserSql = @"
            UPDATE users 
            SET points = @points, 
                currentstreak = @currentStreak, 
                maxstreak = @maxStreak, 
                lastactivitydate = @lastActivityDate
            WHERE id = @userId";
            
        var updateParams = new[]
        {
            new NpgsqlParameter("points", newPoints),
            new NpgsqlParameter("currentStreak", newStreak),
            new NpgsqlParameter("maxStreak", newMaxStreak),
            new NpgsqlParameter("lastActivityDate", DateTime.UtcNow),
            new NpgsqlParameter("userId", completionData.UserId)
        };
        
        await _context.Database.ExecuteSqlRawAsync(updateUserSql, updateParams);

        return Ok(new { 
            message = "Activity completed successfully", 
            pointsEarned = activity.PointsValue,
            currentStreak = newStreak,
            totalPoints = newPoints
        });
    }
    catch (Exception ex)
    {
        // Log the full exception details including inner exceptions
        var fullError = ex.ToString();
        _logger.LogError(ex, "Error occurred while completing activity {Id}: {ErrorMessage}\nFull error: {FullError}", 
            id, ex.Message, fullError);
        return StatusCode(500, new { 
            message = $"An error occurred while completing the activity", 
            error = ex.Message,
            details = ex.InnerException?.Message
        });
    }
}

 // GET: api/activities/completed/{userId}?date=2025-07-10
[HttpGet("completed/{userId}")]
public async Task<ActionResult<IEnumerable<object>>> GetCompletedActivities(int userId, [FromQuery] DateTime? date = null)
{
    try
    {
        _logger.LogInformation("Fetching completed activities for user {UserId}", userId);

        // Use a simpler query that avoids directly referencing the problematic column
        var query = from ac in _context.ActivityCompletions
                   join act in _context.SustainableActivities on ac.ActivityId equals act.Id
                   where ac.UserId == userId
                   select new {
                       id = ac.Id,
                       userId = ac.UserId,
                       activityId = ac.ActivityId,
                       completedAt = ac.CompletedAt,
                       // Instead of directly referencing ac.PointsEarned, use act.PointsValue
                       // which we know exists in the database
                       pointsEarned = act.PointsValue,
                       activity = new {
                           id = act.Id,
                           title = act.Title,
                           description = act.Description,
                           pointsValue = act.PointsValue
                       }
                   };

        // Apply date filter if provided
        if (date.HasValue)
        {
            // Convert date to UTC and ensure Kind is set properly
            var filterDate = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            var nextDate = filterDate.AddDays(1);
            
            _logger.LogInformation("Filtering activities by date range: {StartDate} to {EndDate}", 
                filterDate.ToString("o"), nextDate.ToString("o"));
            
            query = query.Where(ac => ac.completedAt >= filterDate && ac.completedAt < nextDate);
        }

        var completions = await query
            .OrderByDescending(ac => ac.completedAt)
            .ToListAsync();

        return Ok(completions);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while fetching completed activities for user {UserId}: {ErrorMessage}", userId, ex.Message);
        return StatusCode(500, new { message = $"An error occurred while retrieving completed activities", error = ex.Message });
    }
}

    // DTO for activity completion requests
    public class ActivityCompletionDto
    {
        public int UserId { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    private bool ActivityExists(int id)
    {
        return _context.SustainableActivities.Any(e => e.Id == id);
    }
}