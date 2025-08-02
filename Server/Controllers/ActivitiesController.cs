using Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using System.Security.Claims;
using Server.Services.Interfaces;
using Server.Services.Implementations;
using Server.DTOs;
using Server.Data;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivitiesService _activitiesService;
    private readonly ILogger<ActivitiesController> _logger;
    private readonly AppDbContext _context;

    public ActivitiesController(IActivitiesService activitiesService, ILogger<ActivitiesController> logger)
    {
        _activitiesService = activitiesService;
        _logger = logger;
    }

    // GET: api/activities
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetActivities()
    {
        try
        {
            _logger.LogInformation("Fetching all sustainable activities");
            var activities = await _activitiesService.GetAllActivitiesAsync();
            var result = activities.Select(a => new
            {
                id = a.Id,
                title = a.Title,
                description = a.Description,
                category = a.Category,
                pointsValue = a.PointsValue,
                isDaily = a.IsDaily,
                isWeekly = a.IsWeekly,
                isOneTime = a.IsOneTime
            });
            return Ok(result);
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
            var activity = await _activitiesService.GetActivityByIdAsync(id);
            if (activity == null)
            {
                _logger.LogWarning("Activity with ID: {Id} not found", id);
                return NotFound(new { message = $"Activity with ID: {id} not found" });
            }
            return Ok(new
            {
                id = activity.Id,
                title = activity.Title,
                description = activity.Description,
                category = activity.Category,
                pointsValue = activity.PointsValue,
                isDaily = activity.IsDaily,
                isWeekly = activity.IsWeekly,
                isOneTime = activity.IsOneTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching activity with ID: {Id}", id);
            return StatusCode(500, new { message = $"An error occurred while retrieving activity with ID: {id}", error = ex.Message });
        }
    }

    // POST: api/activities
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<object>> CreateActivity(CreateActivityDto activityDto)
    {
        try
        {
            _logger.LogInformation("Admin creating new activity: {Title}", activityDto.Title);

            if (activityDto == null)
                return BadRequest(new { message = "Activity data is required" });

            var activity = new SustainableActivity
            {
                Title = activityDto.Title,
                Description = activityDto.Description,
                Category = activityDto.Category,
                PointsValue = activityDto.PointsValue,
                IsDaily = activityDto.IsDaily,
                IsWeekly = activityDto.IsWeekly,
                IsOneTime = activityDto.IsOneTime
            };

            var created = await _activitiesService.CreateActivityAsync(activity);

            return CreatedAtAction(nameof(GetActivity), new { id = created.Id }, new
            {
                id = created.Id,
                title = created.Title,
                description = created.Description,
                category = created.Category,
                pointsValue = created.PointsValue,
                isDaily = created.IsDaily,
                isWeekly = created.IsWeekly,
                isOneTime = created.IsOneTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating activity: {Error}", ex.Message);
            return StatusCode(500, new { message = "An error occurred while creating the activity", error = ex.Message });
        }
    }

    // PUT: api/activities
    [HttpPut]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<object>> UpdateActivity(UpdateActivityDto activityDto)
    {
        try
        {
            _logger.LogInformation("Admin updating activity: {Id} - {Title}", activityDto.Id, activityDto.Title);

            if (activityDto == null || activityDto.Id <= 0)
                return BadRequest(new { message = "Activity data is invalid" });

            var activity = new SustainableActivity
            {
                Id = activityDto.Id,
                Title = activityDto.Title,
                Description = activityDto.Description,
                Category = activityDto.Category,
                PointsValue = activityDto.PointsValue,
                IsDaily = activityDto.IsDaily,
                IsWeekly = activityDto.IsWeekly,
                IsOneTime = activityDto.IsOneTime
            };

            var updated = await _activitiesService.UpdateActivityAsync(activity);

            if (!updated)
                return NotFound(new { message = $"Activity with ID: {activityDto.Id} not found" });

            return Ok(new
            {
                success = true,
                activity = new
                {
                    id = activity.Id,
                    title = activity.Title,
                    description = activity.Description,
                    category = activity.Category,
                    pointsValue = activity.PointsValue,
                    isDaily = activity.IsDaily,
                    isWeekly = activity.IsWeekly,
                    isOneTime = activity.IsOneTime
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating activity: {Error}", ex.Message);
            return StatusCode(500, new { message = "An error occurred while updating the activity", error = ex.Message });
        }
    }

    // DELETE: api/activities
    [HttpDelete]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteActivityFromBody([FromBody] DeleteActivityDto deleteDto)
    {
        try
        {
            if (deleteDto == null || deleteDto.Id <= 0)
                return BadRequest(new { message = "Valid activity ID is required" });

            _logger.LogInformation("Admin deleting activity with ID: {Id} via body payload", deleteDto.Id);

            var deleted = await _activitiesService.DeleteActivityAsync(deleteDto.Id);

            if (!deleted)
                return NotFound(new { message = $"Activity with ID: {deleteDto.Id} not found" });

            return Ok(new { message = $"Activity with ID: {deleteDto.Id} successfully deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting activity with ID from body: {Error}", ex.Message);
            return StatusCode(500, new { message = "An error occurred while deleting the activity", error = ex.Message });
        }
    }

    // GET: api/activities/category/waste-reduction
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<object>>> GetActivitiesByCategory(string category)
    {
        try
        {
            _logger.LogInformation("Fetching activities by category: {Category}", category);
            var activities = await _activitiesService.GetActivitiesByCategoryAsync(category);
            var result = activities.Select(a => new
            {
                id = a.Id,
                title = a.Title,
                description = a.Description,
                category = a.Category,
                pointsValue = a.PointsValue,
                isDaily = a.IsDaily,
                isWeekly = a.IsWeekly,
                isOneTime = a.IsOneTime
            });
            return Ok(result);
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
            var activities = await _activitiesService.GetDailyActivitiesAsync();
            var result = activities.Select(a => new
            {
                id = a.Id,
                title = a.Title,
                description = a.Description,
                category = a.Category,
                pointsValue = a.PointsValue,
                isDaily = a.IsDaily,
                isWeekly = a.IsWeekly,
                isOneTime = a.IsOneTime
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching daily activities");
            return StatusCode(500, new { message = "An error occurred while retrieving daily activities", error = ex.Message });
        }
    }

    // GET: api/activities/weekly
    [HttpGet("weekly")]
    public async Task<ActionResult<IEnumerable<object>>> GetWeeklyActivities()
    {
        try
        {
            _logger.LogInformation("Fetching weekly activities");
            var activities = await _activitiesService.GetWeeklyActivitiesAsync();
            var result = activities.Select(a => new
            {
                id = a.Id,
                title = a.Title,
                description = a.Description,
                category = a.Category,
                pointsValue = a.PointsValue,
                isDaily = a.IsDaily,
                isWeekly = a.IsWeekly,
                isOneTime = a.IsOneTime
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching weekly activities");
            return StatusCode(500, new { message = "An error occurred while retrieving weekly activities", error = ex.Message });
        }
    }

    // GET: api/activities/points-range?min=10&max=50
    [HttpGet("points-range")]
    public async Task<ActionResult<IEnumerable<object>>> GetActivitiesByPointsRange([FromQuery] int min = 0, [FromQuery] int max = int.MaxValue)
    {
        try
        {
            _logger.LogInformation("Fetching activities in points range: {Min} to {Max}", min, max);
            var activities = await _activitiesService.GetActivitiesByPointsRangeAsync(min, max);
            var result = activities.Select(a => new
            {
                id = a.Id,
                title = a.Title,
                description = a.Description,
                category = a.Category,
                pointsValue = a.PointsValue,
                isDaily = a.IsDaily,
                isWeekly = a.IsWeekly,
                isOneTime = a.IsOneTime
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching activities by points range: {Min} to {Max}", min, max);
            return StatusCode(500, new { message = $"An error occurred while retrieving activities in points range: {min} to {max}", error = ex.Message });
        }
    }

    // POST: api/activities/{id}/complete
    [HttpPost("{id}/complete")]
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
            var userQuery = "SELECT id, points, currentstreak, maxstreak, \"Level\" FROM users WHERE id = @userId";
            var userParam = new NpgsqlParameter("userId", completionData.UserId);
            
            var user = await _context.Users
    .Where(u => u.Id == completionData.UserId)
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
            bool LeveledUp = false;
            
            // Level thresholds
            if (newPoints >= 1000 && user.Level < 4)
            {
                newLevel = 4;
                LeveledUp = newLevel > user.Level;
            }
            else if (newPoints >= 500 && user.Level < 3)
            {
                newLevel = 3;
                LeveledUp = newLevel > user.Level;
            }
            else if (newPoints >= 250 && user.Level < 2)
            {
                newLevel = 2;
                LeveledUp = newLevel > user.Level;
            }
            else if (newPoints >= 100 && user.Level < 1)
            {
                newLevel = 1;
                LeveledUp = newLevel > user.Level;
            }
            
            // Update user stats with direct SQL - now including level
            var updateUserSql = @"
                UPDATE users 
                SET points = @points, 
                    currentstreak = @currentStreak, 
                    maxstreak = @maxStreak, 
                    lastactivitydate = @lastActivityDate,
                    ""Level"" = @Level
                WHERE id = @userId";
                
            var updateParams = new[]
            {
                new NpgsqlParameter("points", newPoints),
                new NpgsqlParameter("currentStreak", newStreak),
                new NpgsqlParameter("maxStreak", newMaxStreak),
                new NpgsqlParameter("lastActivityDate", DateTime.UtcNow),
                new NpgsqlParameter("Level", newLevel),
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
                Level = newLevel,
                LeveledUp = LeveledUp,
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

    // Admin endpoint to review activity completions
    [HttpPatch("review/{completionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReviewActivityCompletion(int completionId, [FromBody] ActivityReviewDto reviewData)
    {
        try
        {
            _logger.LogInformation("Admin reviewing activity completion {CompletionId}", completionId);
            
            // Check if completion exists
            var completionQuery = @"
                SELECT id, userid, activityid, reviewstatus, pointsearned 
                FROM activitycompletions 
                WHERE id = @completionId";
                
            var completionParam = new NpgsqlParameter("completionId", completionId);
            
            // Get the completion record
            ActivityCompletion completion = null;
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = completionQuery;
                command.Parameters.Add(completionParam);

                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        completion = new ActivityCompletion
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            UserId = reader.GetInt32(reader.GetOrdinal("userid")),
                            ActivityId = reader.GetInt32(reader.GetOrdinal("activityid")),
                            ReviewStatus = reader.IsDBNull(reader.GetOrdinal("reviewstatus")) 
                                ? "Pending Review" 
                                : reader.GetString(reader.GetOrdinal("reviewstatus")),
                            PointsEarned = reader.GetInt32(reader.GetOrdinal("pointsearned"))
                        };
                    }
                }
            }
            
            if (completion == null)
            {
                _logger.LogWarning("Activity completion with ID {CompletionId} not found", completionId);
                return NotFound(new { message = $"Activity completion with ID {completionId} not found" });
            }
            
            // Update the review status
            var updateSql = @"
                UPDATE activitycompletions 
                SET reviewstatus = @reviewStatus, 
                    adminnotes = @adminNotes
                WHERE id = @completionId";
                
            var updateParams = new[]
            {
                new NpgsqlParameter("reviewStatus", reviewData.Status),
                new NpgsqlParameter("adminNotes", reviewData.AdminNotes ?? (object)DBNull.Value),
                new NpgsqlParameter("completionId", completionId)
            };
            
            await _context.Database.ExecuteSqlRawAsync(updateSql, updateParams);
            
            // If the status is "Rejected", remove the points from the user
            if (reviewData.Status == "Rejected")
            {
                // Get the user's current points
                var userQuery = "SELECT id, points FROM users WHERE id = @userId";
                var userParam = new NpgsqlParameter("userId", completion.UserId);
                
                var user = await _context.Users
                    .FromSqlRaw(userQuery, userParam)
                    .Select(u => new { u.Id, u.Points })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                    
                if (user != null)
                {
                    // Deduct the points
                    var newPoints = Math.Max(0, user.Points - completion.PointsEarned); // Ensure points don't go below 0
                    
                    // Update user points
                    var updateUserSql = "UPDATE users SET points = @points WHERE id = @userId";
                    var updateUserParams = new[]
                    {
                        new NpgsqlParameter("points", newPoints),
                        new NpgsqlParameter("userId", completion.UserId)
                    };
                    
                    await _context.Database.ExecuteSqlRawAsync(updateUserSql, updateUserParams);
                }
            }

            return Ok(new { 
                message = $"Activity completion reviewed successfully. Status: {reviewData.Status}",
                completionId = completionId,
                status = reviewData.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reviewing activity completion {CompletionId}", completionId);
            return StatusCode(500, new { 
                message = $"An error occurred while reviewing the activity completion", 
                error = ex.Message
            });
        }
    }

    [HttpGet("debug-auth")]
public IActionResult DebugAuth()
{
    var identity = HttpContext.User.Identity;
    var isAuthenticated = identity?.IsAuthenticated ?? false;
    
    var claims = new List<object>();
    
    if (identity is ClaimsIdentity claimsIdentity)
    {
        claims = claimsIdentity.Claims.Select(c => new 
        {
            type = c.Type,
            value = c.Value
        }).ToList<object>();
    }
    
    var isAdmin = HttpContext.User.IsInRole("Admin");
    
    return Ok(new
    {
        isAuthenticated,
        userName = identity?.Name,
        claims,
        isAdmin,
        roleClaimType = ClaimTypes.Role,
        customRoleType = "role"
    });
}

    private bool ActivityExists(int id)
    {
        return _context.SustainableActivities.Any(e => e.Id == id);
    }
}