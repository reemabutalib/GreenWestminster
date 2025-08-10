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
    private readonly IClimatiqService _climatiqService;

    public ActivitiesController(
    IActivitiesService activitiesService,
    ILogger<ActivitiesController> logger,
    AppDbContext context,
    IClimatiqService climatiqService)
    {
        _activitiesService = activitiesService;
        _logger = logger;
        _context = context;
        _climatiqService = climatiqService;
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
[RequestSizeLimit(10 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
public async Task<IActionResult> CompleteActivity(int id, [FromForm] ActivityCompletionDto completionData)
{
    try
    {
        _logger.LogInformation("⏳ Starting activity completion for ActivityId: {ActivityId}", id);

        // Model validation
        if (!ModelState.IsValid)
        {
            var errs = ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .ToDictionary(kv => kv.Key, kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { message = "Validation failed", errors = errs });
        }

        // File checks
        if (completionData.Image == null || completionData.Image.Length == 0)
            return BadRequest(new { message = "Image is required" });

        if (!completionData.Image.ContentType.StartsWith("image/"))
            return BadRequest(new { message = "Only image files are allowed" });

        const long maxBytes = 10 * 1024 * 1024; // 10MB
        if (completionData.Image.Length > maxBytes)
            return BadRequest(new { message = "Image must be 10MB or smaller" });

        _logger.LogInformation("📦 CompletionData received: UserId={UserId}, Quantity={Quantity}, Notes={Notes}",
            completionData.UserId, completionData.Quantity, completionData.Notes);

        var activity = await _context.SustainableActivities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
        {
            _logger.LogWarning("❌ Activity with ID {ActivityId} not found", id);
            return NotFound(new { message = $"Activity with ID {id} not found" });
        }

        var matchedCategory = _climatiqService.GetMatchedCategory(activity.Category);
        _logger.LogInformation("🧽 Raw category: {Raw} → Matched Category: {Matched}", activity.Category, matchedCategory ?? "none");

        // CO₂e calc (Quantity is required and > 0 due to [Range])
        double co2e = 0;
        if (!string.IsNullOrEmpty(matchedCategory))
        {
            co2e = await _climatiqService.CalculateCo2Async(matchedCategory, completionData.Quantity!.Value);
            _logger.LogInformation("✅ Climatiq CO₂e response: {co2e} for category '{category}'", co2e, matchedCategory);
        }
        else
        {
            _logger.LogWarning("⚠️ Category '{Raw}' not recognized for CO₂e calculation", activity.Category);
        }

        var user = await _context.Users
            .Where(u => u.Id == completionData.UserId)
            .Select(u => new { u.Id, u.Points, u.CurrentStreak, u.MaxStreak, u.Level })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (user == null)
        {
            _logger.LogWarning("❌ User with ID {UserId} not found", completionData.UserId);
            return NotFound(new { message = $"User with ID {completionData.UserId} not found" });
        }

        // prevent duplicate for same day
        var today = DateTime.UtcNow.Date;
        var alreadyCompleted = await _context.ActivityCompletions.AnyAsync(ac =>
            ac.UserId == completionData.UserId &&
            ac.ActivityId == id &&
            ac.CompletedAt >= today &&
            ac.CompletedAt < today.AddDays(1));

        if (alreadyCompleted)
        {
            _logger.LogInformation("🚫 Activity already completed today by user {UserId}", completionData.UserId);
            return BadRequest(new { message = "Activity already completed today" });
        }

        // Save image
        string imageFileName = null;
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var fileExtension = Path.GetExtension(completionData.Image.FileName);
        imageFileName = $"activity_{id}_user_{completionData.UserId}_{timestamp}{fileExtension}";
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);
        var filePath = Path.Combine(uploadsDir, imageFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await completionData.Image.CopyToAsync(stream);
        }
        _logger.LogInformation("📸 Image saved as {ImageFileName}", imageFileName);

        // CompletedAt
        var completedAt = completionData.CompletedAt != null
            ? DateTime.SpecifyKind(completionData.CompletedAt.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;

        _logger.LogInformation("📝 Inserting completion record into DB...");

        // Points at submission time = 0 (awarded on approval based on CO₂e)
        var insertSql = @"
            INSERT INTO activitycompletions (
                userid, activityid, completedat, imagepath, notes, 
                reviewstatus, pointsearned, co2e_reduction, ""Quantity"")
            VALUES (
                @userId, @activityId, @completedAt, @imagePath, @notes, 
                @reviewStatus, @pointsEarned, @co2eReduction, @Quantity)";

        var insertParams = new[]
        {
            new NpgsqlParameter("userId", completionData.UserId),
            new NpgsqlParameter("activityId", id),
            new NpgsqlParameter("completedAt", completedAt),
            new NpgsqlParameter("imagePath", imageFileName),
            new NpgsqlParameter("notes", (object?)completionData.Notes ?? DBNull.Value),
            new NpgsqlParameter("reviewStatus", "Pending Review"),
            new NpgsqlParameter("pointsEarned", (object)0), // ← important: no points yet
            new NpgsqlParameter("co2eReduction", co2e),
            new NpgsqlParameter("Quantity", completionData.Quantity!.Value)
        };

        await _context.Database.ExecuteSqlRawAsync(insertSql, insertParams);

        _logger.LogInformation("✅ Activity completed and saved successfully for user {UserId}", completionData.UserId);

        string imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{imageFileName}";

        return Ok(new
        {
            message = "Activity submitted for review",
            reviewStatus = "Pending Review",
            imageUrl,
            co2eReduction = co2e
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Error during activity completion: {ErrorMessage}", ex.Message);
        return StatusCode(500, new
        {
            message = "An error occurred while completing the activity",
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
       ac.imagepath, ac.notes, ac.reviewstatus, ac.pointsearned,
       ac.Quantity, ac.co2e_reduction,  
       sa.id AS activity_id, sa.title AS activity_title, 
       sa.description AS activity_description, sa.pointsvalue AS activity_pointsvalue
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

                            // ✅ NEW FIELDS
                            Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Quantity")),
                            co2eReduction = reader.IsDBNull(reader.GetOrdinal("co2e_reduction")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("co2e_reduction")),

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

        var completion = await _context.ActivityCompletions
            .FirstOrDefaultAsync(ac => ac.Id == completionId);

        if (completion == null)
        {
            _logger.LogWarning("Completion ID {CompletionId} not found", completionId);
            return NotFound(new { message = $"Completion with ID {completionId} not found" });
        }

        completion.ReviewStatus = reviewData.Status;
        completion.AdminNotes = reviewData.AdminNotes;

        if (reviewData.Status == "Approved")
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == completion.UserId);
            if (user != null)
            {
                // Points from CO₂e only (no fallback)
                var co2 = completion.Co2eReduction ?? 0;
                var earnedPoints = (int)Math.Round(co2 * 100.0, MidpointRounding.AwayFromZero); // 1 pt / 0.01 kg

                completion.PointsEarned = earnedPoints;
                user.Points += earnedPoints;

                // Streak logic (unchanged)
                var today = DateTime.UtcNow.Date;
                var yesterday = today.AddDays(-1);
                var hadActivityYesterday = await _context.ActivityCompletions.AnyAsync(ac =>
                    ac.UserId == user.Id &&
                    ac.ReviewStatus == "Approved" &&
                    ac.CompletedAt >= yesterday &&
                    ac.CompletedAt < today
                );

                int newStreak = hadActivityYesterday ? user.CurrentStreak + 1 : 1;
                user.CurrentStreak = newStreak;
                user.MaxStreak = Math.Max(user.MaxStreak, newStreak);
                user.LastActivityDate = DateTime.UtcNow;

                // Optional: keep your existing level logic, or switch levels to be based on total points
                _context.Users.Update(user);
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = completion.Id,
            userId = completion.UserId,
            activityId = completion.ActivityId,
            status = reviewData.Status,
            adminNotes = reviewData.AdminNotes,
            pointsEarned = completion.PointsEarned
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error reviewing activity completion {CompletionId}", completionId);
        return StatusCode(500, new { message = "Error reviewing activity", error = ex.Message });
    }
}



  [HttpGet("users/{userId}/carbon-impact")]
public async Task<IActionResult> GetCarbonImpact(int userId)
{
    var completions = await _context.ActivityCompletions
        .Include(ac => ac.Activity)
        .Where(ac => ac.UserId == userId)
        .ToListAsync();

    double totalCO2 = completions.Sum(c => c.Co2eReduction ?? 0);
    double totalWater = completions
        .Where(c => c.Activity.Category.ToLower().Contains("water"))
        .Sum(c => c.Quantity ?? 0);

    return Ok(new
    {
        co2Reduced = Math.Round(totalCO2, 2),
        treesEquivalent = Math.Round(totalCO2 / 21.0, 2),
        waterSaved = Math.Round(totalWater, 2)
    });
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