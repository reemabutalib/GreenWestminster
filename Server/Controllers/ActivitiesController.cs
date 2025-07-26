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
public async Task<ActionResult<object>> CreateActivity(CreateActivityDto activityDto)
{
    try
    {
        _logger.LogInformation("Creating new activity: {Title}", activityDto.Title);
        
        if (activityDto == null)
        {
            return BadRequest(new { message = "Activity data is required" });
        }
        
        // Create activity with only the fields we know exist in the database
        var sql = @"
            INSERT INTO sustainableactivities 
                (title, description, category, pointsvalue, isdaily, isweekly, isonetime)
            VALUES 
                (@title, @description, @category, @pointsValue, @isDaily, @isWeekly, @isOneTime)
            RETURNING id";
        
        var parameters = new[]
        {
            new NpgsqlParameter("title", activityDto.Title),
            new NpgsqlParameter("description", activityDto.Description),
            new NpgsqlParameter("category", activityDto.Category),
            new NpgsqlParameter("pointsValue", activityDto.PointsValue),
            new NpgsqlParameter("isDaily", activityDto.IsDaily),
            new NpgsqlParameter("isWeekly", activityDto.IsWeekly),
            new NpgsqlParameter("isOneTime", activityDto.IsOneTime)
        };
        
        // Execute the SQL directly to bypass EF Core's mapping
        int newActivityId;
        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }

            if (command.Connection.State != System.Data.ConnectionState.Open)
            {
                command.Connection.Open();
            }

            // Get the ID of the newly inserted activity
            newActivityId = Convert.ToInt32(await command.ExecuteScalarAsync());
        }
        
        // Return the created activity
        var createdActivity = new
        {
            id = newActivityId,
            title = activityDto.Title,
            description = activityDto.Description,
            category = activityDto.Category,
            pointsValue = activityDto.PointsValue,
            isDaily = activityDto.IsDaily,
            isWeekly = activityDto.IsWeekly,
            isOneTime = activityDto.IsOneTime
        };

        return CreatedAtAction(nameof(GetActivity), new { id = newActivityId }, createdActivity);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while creating activity: {Error}", ex.Message);
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
[RequestSizeLimit(10 * 1024 * 1024)] // Limit to 10MB
[RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
public async Task<IActionResult> CompleteActivity(int id, [FromForm] ActivityCompletionDto completionData)
{
    try
    {
        _logger.LogInformation("Marking activity {Id} as completed for user {UserId}", id, completionData.UserId);
        
        // Get activity data using direct SQL
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

        // Get the user with a direct query
        var userQuery = "SELECT id, points, currentstreak, maxstreak, level FROM users WHERE id = @userId";
        var userParam = new NpgsqlParameter("userId", completionData.UserId);
        
        var user = await _context.Users
            .FromSqlRaw(userQuery, userParam)
            .Select(u => new { u.Id, u.Points, u.CurrentStreak, u.MaxStreak, u.Level })
            .AsNoTracking()
            .FirstOrDefaultAsync();
            
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found during activity completion", completionData.UserId);
            return NotFound(new { message = $"User with ID {completionData.UserId} not found" });
        }

        // Check if already completed today
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

        // Handle image upload if provided
        string imageFileName = null;
        if (completionData.Image != null && completionData.Image.Length > 0)
        {
            // Generate a unique filename with timestamp
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var fileExtension = Path.GetExtension(completionData.Image.FileName);
            imageFileName = $"activity_{id}_user_{completionData.UserId}_{timestamp}{fileExtension}";
            
            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }
            
            // Save the file
            var filePath = Path.Combine(uploadsDir, imageFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await completionData.Image.CopyToAsync(stream);
            }
            
            _logger.LogInformation("Saved image {ImageFileName} for activity completion", imageFileName);
        }

        // Insert completion record
        var completedAt = completionData.CompletedAt != null 
            ? DateTime.SpecifyKind(completionData.CompletedAt.Value, DateTimeKind.Utc) 
            : DateTime.UtcNow;
        
        // Ensure the table has the necessary columns
        try
        {
            // Check if the columns exist before trying to add them
            var checkColumnsQuery = @"
                SELECT column_name 
                FROM information_schema.columns 
                WHERE table_name = 'activitycompletions' 
                  AND column_name IN ('imagepath', 'notes', 'reviewstatus')";
            
            var existingColumns = await _context.Database
                .SqlQueryRaw<string>(checkColumnsQuery)
                .ToListAsync();
                
            if (!existingColumns.Contains("imagepath"))
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE activitycompletions ADD COLUMN imagepath text");
            }
            
            if (!existingColumns.Contains("notes"))
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE activitycompletions ADD COLUMN notes text");
            }
            
            if (!existingColumns.Contains("reviewstatus"))
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE activitycompletions ADD COLUMN reviewstatus varchar(20) DEFAULT 'Pending Review'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking/adding columns to activitycompletions table. " +
                "This is expected if columns already exist");
            // Continue anyway - if columns exist, this is fine
        }

        // Insert completion record with all fields
        var insertSql = @"
            INSERT INTO activitycompletions (userid, activityid, completedat, imagepath, notes, reviewstatus, pointsearned) 
            VALUES (@userId, @activityId, @completedAt, @imagePath, @notes, @reviewStatus, @pointsEarned)";
            
        var insertParams = new[]
        {
            new NpgsqlParameter("userId", completionData.UserId),
            new NpgsqlParameter("activityId", id),
            new NpgsqlParameter("completedAt", completedAt),
            new NpgsqlParameter("imagePath", imageFileName ?? (object)DBNull.Value),
            new NpgsqlParameter("notes", completionData.Notes ?? (object)DBNull.Value),
            new NpgsqlParameter("reviewStatus", "Pending Review"),
            new NpgsqlParameter("pointsEarned", activity.PointsValue)
        };
        
        await _context.Database.ExecuteSqlRawAsync(insertSql, insertParams);

        // Check for streak updates
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
        
        // Calculate streak
        int newStreak = 1;
        if (yesterdayCount > 0)
        {
            newStreak = user.CurrentStreak + 1;
        }
        
        int newMaxStreak = Math.Max(newStreak, user.MaxStreak);
        int newPoints = user.Points + activity.PointsValue;
        
        // Calculate the new level based on points thresholds
        int newLevel = user.Level;
        bool leveledUp = false;
        
        // Level thresholds
        if (newPoints >= 1000 && user.Level < 4)
        {
            newLevel = 4;
            leveledUp = newLevel > user.Level;
        }
        else if (newPoints >= 500 && user.Level < 3)
        {
            newLevel = 3;
            leveledUp = newLevel > user.Level;
        }
        else if (newPoints >= 250 && user.Level < 2)
        {
            newLevel = 2;
            leveledUp = newLevel > user.Level;
        }
        else if (newPoints >= 100 && user.Level < 1)
        {
            newLevel = 1;
            leveledUp = newLevel > user.Level;
        }
        
        // Update user stats with direct SQL - now including level
        var updateUserSql = @"
            UPDATE users 
            SET points = @points, 
                currentstreak = @currentStreak, 
                maxstreak = @maxStreak, 
                lastactivitydate = @lastActivityDate,
                level = @level
            WHERE id = @userId";
            
        var updateParams = new[]
        {
            new NpgsqlParameter("points", newPoints),
            new NpgsqlParameter("currentStreak", newStreak),
            new NpgsqlParameter("maxStreak", newMaxStreak),
            new NpgsqlParameter("lastActivityDate", DateTime.UtcNow),
            new NpgsqlParameter("level", newLevel),
            new NpgsqlParameter("userId", completionData.UserId)
        };
        
        await _context.Database.ExecuteSqlRawAsync(updateUserSql, updateParams);

        // Return the URL for the uploaded image if available
        string imageUrl = null;
        if (!string.IsNullOrEmpty(imageFileName))
        {
            // Construct a URL to access the image
            imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{imageFileName}";
        }

        // Calculate points needed for next level
        int pointsToNextLevel = 0;
        if (newLevel < 4)
        {
            // Calculate points needed for next level
            if (newLevel == 1)
                pointsToNextLevel = 250 - newPoints;
            else if (newLevel == 2)
                pointsToNextLevel = 500 - newPoints;
            else if (newLevel == 3)
                pointsToNextLevel = 1000 - newPoints;
        }

        return Ok(new { 
            message = "Activity completed successfully", 
            pointsEarned = activity.PointsValue,
            currentStreak = newStreak,
            totalPoints = newPoints,
            imageUrl = imageUrl,
            reviewStatus = "Pending Review",
            level = newLevel,
            leveledUp = leveledUp,
            pointsToNextLevel = pointsToNextLevel
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

    // GET: api/activities/completed/{userId}
    [HttpGet("completed/{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetCompletedActivities(int userId, [FromQuery] DateTime? date = null)
    {
        try
        {
            _logger.LogInformation("Fetching completed activities for user {UserId}", userId);

            // Use raw SQL to avoid EF Core mapping issues with dynamically added columns
            var sql = @"
            SELECT ac.id, ac.userid, ac.activityid, ac.completedat, 
                   ac.imagepath, ac.notes, ac.reviewstatus,
                   sa.pointsvalue as pointsearned,
                   sa.id as activity_id, sa.title as activity_title, 
                   sa.description as activity_description, sa.pointsvalue as activity_pointsvalue
            FROM activitycompletions ac
            JOIN sustainableactivities sa ON ac.activityid = sa.id
            WHERE ac.userid = @userId";

            var parameters = new List<NpgsqlParameter> { new NpgsqlParameter("userId", userId) };

            // Add date filter if provided
            if (date.HasValue)
            {
                // Convert date to UTC and ensure Kind is set properly
                var filterDate = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
                var nextDate = filterDate.AddDays(1);

                _logger.LogInformation("Filtering activities by date range: {StartDate} to {EndDate}",
                    filterDate.ToString("o"), nextDate.ToString("o"));

                sql += " AND ac.completedat >= @startDate AND ac.completedat < @endDate";
                parameters.Add(new NpgsqlParameter("startDate", filterDate));
                parameters.Add(new NpgsqlParameter("endDate", nextDate));
            }

            sql += " ORDER BY ac.completedat DESC";

            var completions = new List<dynamic>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }

                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var completion = new
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            userId = reader.GetInt32(reader.GetOrdinal("userid")),
                            activityId = reader.GetInt32(reader.GetOrdinal("activityid")),
                            completedAt = reader.GetDateTime(reader.GetOrdinal("completedat")),
                            imagePath = reader.IsDBNull(reader.GetOrdinal("imagepath")) ? null : reader.GetString(reader.GetOrdinal("imagepath")),
                            notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                            reviewStatus = reader.IsDBNull(reader.GetOrdinal("reviewstatus")) ? "Pending Review" : reader.GetString(reader.GetOrdinal("reviewstatus")),
                            pointsEarned = reader.GetInt32(reader.GetOrdinal("pointsearned")),
                            activity = new
                            {
                                id = reader.GetInt32(reader.GetOrdinal("activity_id")),
                                title = reader.GetString(reader.GetOrdinal("activity_title")),
                                description = reader.GetString(reader.GetOrdinal("activity_description")),
                                pointsValue = reader.GetInt32(reader.GetOrdinal("activity_pointsvalue"))
                            }
                        };

                        completions.Add(completion);
                    }
                }
            }

            // Transform the results to include full image URLs
            var result = completions.Select(c => new
            {
                id = c.id,
                userId = c.userId,
                activityId = c.activityId,
                completedAt = c.completedAt,
                notes = c.notes,
                reviewStatus = c.reviewStatus,
                imageUrl = !string.IsNullOrEmpty(c.imagePath)
                    ? $"{Request.Scheme}://{Request.Host}/uploads/{c.imagePath}"
                    : null,
                pointsEarned = c.pointsEarned,
                activity = c.activity
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching completed activities for user {UserId}: {ErrorMessage}", userId, ex.Message);
            return StatusCode(500, new { message = $"An error occurred while retrieving completed activities", error = ex.Message });
        }
    }

// GET: api/activities/streak/{userId}
[HttpGet("streak/{userId}")]
public async Task<ActionResult<object>> GetUserStreak(int userId)
{
    try
    {
        _logger.LogInformation("Fetching streak information for user {UserId}", userId);
        
        // First check if the user exists
        var userQuery = "SELECT id, points, currentstreak, maxstreak, lastactivitydate FROM users WHERE id = @userId";
        var userParam = new NpgsqlParameter("userId", userId);
        
        var user = await _context.Users
            .FromSqlRaw(userQuery, userParam)
            .Select(u => new { 
                u.Id, 
                u.Points, 
                u.CurrentStreak, 
                u.MaxStreak, 
                u.LastActivityDate 
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
            
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found when fetching streak", userId);
            return NotFound(new { message = $"User with ID {userId} not found" });
        }

        // Calculate if streak is still valid (has to log activity daily)
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        bool streakBroken = false;
        
        // Check if user has completed any activity yesterday or today
        if (user.LastActivityDate != null)  
{
        // Access the Date property directly if LastActivityDate is a DateTime,
        // or use .Value.Date if it's a nullable DateTime?
        var lastActivityDate = user.LastActivityDate.Date; 
            
            // If last activity was before yesterday, streak is broken
            if (lastActivityDate < yesterday)
            {
                streakBroken = true;
                
                // Update user's current streak to 0 in database
                var updateStreakSql = @"
                    UPDATE users 
                    SET currentstreak = 0
                    WHERE id = @userId";
                    
                var updateParam = new NpgsqlParameter("userId", userId);
                
                await _context.Database.ExecuteSqlRawAsync(updateStreakSql, updateParam);
                
                // Set current streak to 0 for the response
                user = user with { CurrentStreak = 0 };
            }
        }
        else
        {
            // If user has no last activity date recorded, they have no streak
            streakBroken = true;
        }

        // Get activity completion history for analytics
        var lastWeekStart = today.AddDays(-6); // Last 7 days including today
        
        var activityQuery = @"
            SELECT 
                DATE(completedat) as activity_date, 
                COUNT(*) as completion_count
            FROM 
                activitycompletions
            WHERE 
                userid = @userId AND
                completedat >= @startDate
            GROUP BY 
                DATE(completedat)
            ORDER BY 
                activity_date";
                
        var activityParams = new[]
        {
            new NpgsqlParameter("userId", userId),
            new NpgsqlParameter("startDate", lastWeekStart)
        };
        
        // Get activity completion records for streak analysis
        var activityCompletions = new List<dynamic>();
        
        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = activityQuery;
            foreach (var param in activityParams)
            {
                command.Parameters.Add(param);
            }

            if (command.Connection.State != System.Data.ConnectionState.Open)
            {
                command.Connection.Open();
            }

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var completion = new
                    {
                        Date = reader.GetDateTime(reader.GetOrdinal("activity_date")),
                        Count = reader.GetInt32(reader.GetOrdinal("completion_count"))
                    };
                    
                    activityCompletions.Add(completion);
                }
            }
        }

        // Create streak calendar (last 7 days with activity status)
        var calendar = new List<object>();
        for (int i = 0; i < 7; i++)
        {
            var day = today.AddDays(-6 + i);
            var activityForDay = activityCompletions.FirstOrDefault(c => c.Date.Date == day);
            
            calendar.Add(new {
                date = day.ToString("yyyy-MM-dd"),
                dayOfWeek = day.DayOfWeek.ToString(),
                hasActivity = activityForDay != null,
                completionCount = activityForDay?.Count ?? 0
            });
        }

        // Return streak information
        return Ok(new { 
            currentStreak = user.CurrentStreak,
            maxStreak = user.MaxStreak,
            streakBroken = streakBroken,
            lastActivityDate = user.LastActivityDate,
            activityCalendar = calendar
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while fetching streak for user {UserId}: {ErrorMessage}", 
            userId, ex.Message);
        return StatusCode(500, new { 
            message = $"An error occurred while retrieving streak information", 
            error = ex.Message
        });
    }
}

    // DTO for activity completion requests
    public class ActivityCompletionDto
    {
        public int UserId { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Make notes optional
        public string? Notes { get; set; } = null;

        // Make image optional
        public IFormFile? Image { get; set; } = null;
    }

public class CreateActivityDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsValue { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsDaily { get; set; }
    public bool IsWeekly { get; set; }
    public bool IsOneTime { get; set; }
    // Only include the properties that exist in your database
}

    private bool ActivityExists(int id)
    {
        return _context.SustainableActivities.Any(e => e.Id == id);
    }
}