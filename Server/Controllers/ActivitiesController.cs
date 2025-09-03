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

    // POST: api/activities/{id}/complete
    [HttpPost("{id}/complete")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public async Task<IActionResult> CompleteActivity(int id, [FromForm] ActivityCompletionDto completionData)
    {
        try
        {
            _logger.LogInformation("⏳ Starting activity completion for ActivityId: {ActivityId}", id);

            // ModelState validation (shows which fields failed)
            if (!ModelState.IsValid)
            {
                var errs = ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new { message = "Validation failed", errors = errs });
            }

            // Server-side required checks (in case DTO attributes aren't present or client bypasses them)
            if (completionData.Quantity == null || completionData.Quantity <= 0)
                return BadRequest(new { message = "Quantity is required and must be greater than 0." });

            if (completionData.Image == null || completionData.Image.Length == 0)
                return BadRequest(new { message = "Image is required." });

            if (!completionData.Image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Only image files are allowed." });

            const long maxBytes = 10 * 1024 * 1024; // 10MB
            if (completionData.Image.Length > maxBytes)
                return BadRequest(new { message = "Image must be 10MB or smaller." });

            _logger.LogInformation("📦 CompletionData received: UserId={UserId}, Quantity={Quantity}, Notes={Notes}",
                completionData.UserId, completionData.Quantity, completionData.Notes);

            // Load activity
            var activity = await _context.SustainableActivities
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activity == null)
            {
                _logger.LogWarning("❌ Activity with ID {ActivityId} not found", id);
                return NotFound(new { message = $"Activity with ID {id} not found" });
            }

            // Map category for Climatiq
            var matchedCategory = _climatiqService.GetMatchedCategory(activity.Category);
            _logger.LogInformation("🧽 Raw category: {Raw} → Matched Category: {Matched}", activity.Category, matchedCategory ?? "none");

            // Calculate CO₂e
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

            // Ensure user exists
            var userExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == completionData.UserId);

            if (!userExists)
            {
                _logger.LogWarning("❌ User with ID {UserId} not found", completionData.UserId);
                return NotFound(new { message = $"User with ID {completionData.UserId} not found" });
            }

            // Normalize CompletedAt (use provided date, else now)
            var completedAt = completionData.CompletedAt != null
                ? DateTime.SpecifyKind(completionData.CompletedAt.Value, DateTimeKind.Utc)
                : DateTime.UtcNow;

            var dayStart = completedAt.Date;
            var dayEnd = dayStart.AddDays(1);

            // Prevent duplicates for the same user/activity on the same selected date
            var alreadyCompleted = await _context.ActivityCompletions.AnyAsync(ac =>
                ac.UserId == completionData.UserId &&
                ac.ActivityId == id &&
                ac.CompletedAt >= dayStart &&
                ac.CompletedAt < dayEnd);

            if (alreadyCompleted)
            {
                _logger.LogInformation("🚫 Activity {ActivityId} already completed on {Date} by user {UserId}",
                    id, dayStart.ToString("yyyy-MM-dd"), completionData.UserId);
                return BadRequest(new { message = "You already logged this activity for that date." });
            }

            // Save image to disk
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var fileExtension = Path.GetExtension(completionData.Image.FileName);
            var imageFileName = $"activity_{id}_user_{completionData.UserId}_{timestamp}{fileExtension}";
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, imageFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await completionData.Image.CopyToAsync(stream);
            }
            _logger.LogInformation("📸 Image saved as {ImageFileName}", imageFileName);

            _logger.LogInformation("📝 Inserting completion record into DB...");

            // Store with 0 points (awarded on approval based on CO₂e: 0.5 kg = 1 point)
            var insertSql = @"
            INSERT INTO activitycompletions (
                userid, activityid, completedat, imagepath, notes, 
                reviewstatus, pointsearned, co2e_reduction, quantity)
            VALUES (
                @userId, @activityId, @completedAt, @imagePath, @notes, 
                @reviewStatus, @pointsEarned, @co2eReduction, @quantity)";

            var insertParams = new[]
            {
            new Npgsql.NpgsqlParameter("userId", completionData.UserId),
            new Npgsql.NpgsqlParameter("activityId", id),
            new Npgsql.NpgsqlParameter("completedAt", completedAt),
            new Npgsql.NpgsqlParameter("imagePath", imageFileName),
            new Npgsql.NpgsqlParameter("notes", (object?)completionData.Notes ?? DBNull.Value),
            new Npgsql.NpgsqlParameter("reviewStatus", "Pending Review"),
            new Npgsql.NpgsqlParameter("pointsEarned", (object)0), // keep 0 at submission
            new Npgsql.NpgsqlParameter("co2eReduction", co2e),
            new Npgsql.NpgsqlParameter("quantity", completionData.Quantity!.Value)
        };

            await _context.Database.ExecuteSqlRawAsync(insertSql, insertParams);

            _logger.LogInformation("✅ Activity completed and saved successfully for user {UserId}", completionData.UserId);

            var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{imageFileName}";

            return Ok(new
            {
                message = "Activity submitted for review",
                reviewStatus = "Pending Review",
                imageUrl,
                co2eReduction = co2e,
                waterSaved = (activity.Category != null && activity.Category.ToLower().Contains("water"))
            ? completionData.Quantity
            : 0
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
public async Task<ActionResult<IEnumerable<object>>> GetCompletedActivities(
    int userId, [FromQuery] DateTime? date = null, [FromQuery] string? status = null)
{
    try
    {
        _logger.LogInformation("Fetching all activity completions for user {UserId}", userId);

        var sql = @"
            SELECT ac.id, ac.userid, ac.activityid, ac.completedat,
                   ac.imagepath, ac.notes, ac.reviewstatus, ac.adminnotes,
                   ac.pointsearned, ac.quantity, ac.co2e_reduction,  
                   sa.id AS activity_id, sa.title AS activity_title, 
                   sa.description AS activity_description, sa.pointsvalue AS activity_pointsvalue
            FROM activitycompletions ac
            JOIN sustainableactivities sa ON ac.activityid = sa.id
            WHERE ac.userid = @userId";

        var parameters = new List<NpgsqlParameter> { new NpgsqlParameter("userId", userId) };

        if (date.HasValue)
        {
            var filterDate = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            var nextDate = filterDate.AddDays(1);
            sql += " AND ac.completedat >= @startDate AND ac.completedat < @endDate";
            parameters.Add(new NpgsqlParameter("startDate", filterDate));
            parameters.Add(new NpgsqlParameter("endDate", nextDate));
        }

        sql += " ORDER BY ac.completedat DESC";

        var completions = new List<dynamic>();

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
            foreach (var param in parameters) command.Parameters.Add(param);

            if (command.Connection.State != System.Data.ConnectionState.Open)
                command.Connection.Open();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    completions.Add(new
                    {
                        id = reader.GetInt32(reader.GetOrdinal("id")),
                        userId = reader.GetInt32(reader.GetOrdinal("userid")),
                        activityId = reader.GetInt32(reader.GetOrdinal("activityid")),
                        completedAt = reader.GetDateTime(reader.GetOrdinal("completedat")),
                        imagePath = reader.IsDBNull(reader.GetOrdinal("imagepath")) ? null : reader.GetString(reader.GetOrdinal("imagepath")),
                        notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                        reviewStatus = reader.IsDBNull(reader.GetOrdinal("reviewstatus")) ? "Pending Review" : reader.GetString(reader.GetOrdinal("reviewstatus")),
                        adminNotes = reader.IsDBNull(reader.GetOrdinal("adminnotes")) ? null : reader.GetString(reader.GetOrdinal("adminnotes")),
                        pointsEarned = reader.GetInt32(reader.GetOrdinal("pointsearned")),
                        // ⬇⬇⬇ lowercase field name here
                        quantity = reader.IsDBNull(reader.GetOrdinal("quantity")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("quantity")),
                        co2eReduction = reader.IsDBNull(reader.GetOrdinal("co2e_reduction")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("co2e_reduction")),
                        activity = new
                        {
                            id = reader.GetInt32(reader.GetOrdinal("activity_id")),
                            title = reader.GetString(reader.GetOrdinal("activity_title")),
                            description = reader.GetString(reader.GetOrdinal("activity_description")),
                            pointsValue = reader.GetInt32(reader.GetOrdinal("activity_pointsvalue"))
                        }
                    });
                }
            }
        }

        var result = completions.Select(c => new
        {
            id = c.id,
            userId = c.userId,
            activityId = c.activityId,
            completedAt = c.completedAt,
            notes = c.notes,
            reviewStatus = c.reviewStatus,
            adminNotes = c.adminNotes,
            imageUrl = !string.IsNullOrEmpty(c.imagePath)
                ? $"{Request.Scheme}://{Request.Host}/uploads/{c.imagePath}"
                : null,
            pointsEarned = c.pointsEarned,
            quantity = c.quantity,
            co2eReduction = c.co2eReduction,
            activity = c.activity
        });

        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while fetching completions for user {UserId}", userId);
        return StatusCode(500, new { message = "An error occurred while retrieving completed activities", error = ex.Message, inner = ex.InnerException?.Message });
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
                .Select(u => new
                {
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
               var lastActivityDate = user.LastActivityDate?.Date;

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

                calendar.Add(new
                {
                    date = day.ToString("yyyy-MM-dd"),
                    dayOfWeek = day.DayOfWeek.ToString(),
                    hasActivity = activityForDay != null,
                    completionCount = activityForDay?.Count ?? 0
                });
            }

            // Return streak information
            return Ok(new
            {
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
            return StatusCode(500, new
            {
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
    if (reviewData == null || string.IsNullOrWhiteSpace(reviewData.Status))
        return BadRequest(new { message = "Status is required." });

    var status = reviewData.Status.Trim();
    if (!status.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
        !status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
    {
        return BadRequest(new { message = "Status must be 'Approved' or 'Rejected'." });
    }

    // Load completion + activity + user (we need them all)
    var completion = await _context.ActivityCompletions
        .Include(c => c.Activity)
        .FirstOrDefaultAsync(c => c.Id == completionId);

    if (completion == null)
        return NotFound(new { message = $"Completion with ID {completionId} not found" });

    var previousStatus = completion.ReviewStatus ?? "Pending Review";
    var toApproved   = !previousStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
                       status.Equals("Approved",   StringComparison.OrdinalIgnoreCase);
    var toRejected   =  previousStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
                       status.Equals("Rejected",   StringComparison.OrdinalIgnoreCase);

    // We'll use the completion's date for streak logic (NOT 'now')
    var completionDate = DateTime.SpecifyKind(completion.CompletedAt, DateTimeKind.Utc).Date;

    using var tx = await _context.Database.BeginTransactionAsync();
    try
    {
        // --- Points awarding ---
        // You were storing PointsEarned=0 at submit; award on approval:
        if (toApproved)
        {
            // CO2 points: 1 point per 0.5kg (floor), never negative
            var co2 = completion.Co2eReduction ?? 0d;
            var co2Pts = Math.Max(0, (int)Math.Floor(co2 / 0.5));

            // Water points (optional rule): 1 pt per 10 liters for water category
            var waterPts = 0;
            if (completion.Activity?.Category != null &&
                completion.Activity.Category.Contains("water", StringComparison.OrdinalIgnoreCase))
            {
                var qty = completion.Quantity ?? 0d;
                waterPts = Math.Max(0, (int)Math.Floor(qty / 10d));
            }

            completion.PointsEarned = co2Pts + waterPts;
        }

        // If moving away from Approved, remove previously granted points
        if (toRejected)
            completion.PointsEarned = 0;

        completion.ReviewStatus = status;
        completion.AdminNotes   = reviewData.AdminNotes;

        _context.ActivityCompletions.Update(completion);
        await _context.SaveChangesAsync();

        // --- Update user points and streak ---
        var user = await _context.Users.FirstAsync(u => u.Id == completion.UserId);

        // Points
        if (toApproved)
        {
            user.Points += completion.PointsEarned;
        }
        else if (toRejected && previousStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            // roll back points if you ever allow approve->reject
            user.Points = Math.Max(0, user.Points - completion.PointsEarned);
        }

        // Streak (only on transition to Approved)
        if (toApproved)
        {
            // If the user already has last activity recorded for the same date, don't increment
            var last = user.LastActivityDate?.Date;

            if (last == null)
            {
                user.CurrentStreak = 1;
                user.MaxStreak = Math.Max(user.MaxStreak, user.CurrentStreak);
                user.LastActivityDate = completionDate;
            }
            else if (last == completionDate)
            {
                // Already counted this day (another approval from same day) -> no change
            }
            else if (last.Value == completionDate.AddDays(-1))
            {
                // consecutive day -> increment
                user.CurrentStreak += 1;
                user.MaxStreak = Math.Max(user.MaxStreak, user.CurrentStreak);
                user.LastActivityDate = completionDate;
            }
            else if (last.Value < completionDate)
            {
                // gap or out of order -> start new streak at 1 for that day
                user.CurrentStreak = 1;
                user.MaxStreak = Math.Max(user.MaxStreak, user.CurrentStreak);
                user.LastActivityDate = completionDate;
            }
            // If last > completionDate (approving an older backfilled item),
            // do not change streak (keeps monotonic date semantics).
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        // Return a lean DTO
        return Ok(new
        {
            id = completion.Id,
            userId = completion.UserId,
            activityId = completion.ActivityId,
            status = completion.ReviewStatus,
            adminNotes = completion.AdminNotes,
            pointsEarned = completion.PointsEarned,
            userPoints = user.Points,
            userCurrentStreak = user.CurrentStreak,
            userMaxStreak = user.MaxStreak,
            userLastActivityDate = user.LastActivityDate
        });
    }
    catch (Exception ex)
    {
        await tx.RollbackAsync();
        _logger.LogError(ex, "Error reviewing completion {CompletionId}", completionId);
        return StatusCode(500, new { message = "Error reviewing activity", error = ex.Message });
    }
}



// GET: api/activities/users/{userId}/carbon-impact
[HttpGet("users/{userId}/carbon-impact")]
public async Task<IActionResult> GetCarbonImpact(int userId)
{
    var completions = await _context.ActivityCompletions
        .Include(ac => ac.Activity)
        .Where(ac => ac.UserId == userId && ac.ReviewStatus == "Approved")
        .ToListAsync();

    double totalCO2 = completions.Sum(c => c.Co2eReduction ?? 0);
    double totalWater = completions
        .Where(c => c.Activity != null && 
                    !string.IsNullOrEmpty(c.Activity.Category) &&
                    c.Activity.Category.Contains("water", StringComparison.OrdinalIgnoreCase))
        .Sum(c => c.Quantity ?? 0);

    return Ok(new
    {
        co2Reduced = Math.Round(totalCO2, 2),
        treesEquivalent = Math.Round(totalCO2 / 21.0, 2),
        waterSaved = Math.Round(totalWater, 2)
    });
}


// GET: api/activities/pending/{userId}
[HttpGet("pending/{userId}")]
public async Task<ActionResult<IEnumerable<object>>> GetPendingActivities(int userId)
{
    try
    {
        var baseUrl = (Request?.Scheme != null && Request?.Host.HasValue == true)
            ? $"{Request.Scheme}://{Request.Host}"
            : "http://localhost:5138";

        var items = await (
            from ac in _context.ActivityCompletions.AsNoTracking()
            join sa in _context.SustainableActivities.AsNoTracking()
                on ac.ActivityId equals sa.Id into saj
            from sa in saj.DefaultIfEmpty()
            where ac.UserId == userId
               && (ac.ReviewStatus == "Pending Review" || ac.ReviewStatus == "Rejected")
            orderby ac.CompletedAt descending
            select new
            {
                id = ac.Id,
                activityId = ac.ActivityId,
                title = sa != null ? sa.Title : "Unknown",
                completedAt = ac.CompletedAt,
                reviewStatus = ac.ReviewStatus ?? "Pending Review",
                // 🔧 coalesce possible NULLs to empty strings to avoid InvalidCastException
                notes = ac.Notes ?? string.Empty,
                adminNotes = ac.AdminNotes ?? string.Empty,
                imageUrl = !string.IsNullOrWhiteSpace(ac.ImagePath)
                    ? $"{baseUrl}/uploads/{ac.ImagePath}"
                    : null
            }
        ).ToListAsync();

        return Ok(items);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching pending+rejected for user {UserId}", userId);
        return StatusCode(500, new { message = "An error occurred while retrieving items", error = ex.Message, inner = ex.InnerException?.Message });
    }
}





// POST: api/activities/{completionId}/resubmit
// Lets a user resubmit a previously REJECTED completion (optionally with a new image)
[HttpPost("{completionId}/resubmit")]
[Authorize]
[RequestSizeLimit(10 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
public async Task<IActionResult> ResubmitActivity(int completionId, [FromForm] ResubmitActivityDto dto)
{
    try
    {
        if (dto == null || dto.UserId <= 0)
            return BadRequest(new { message = "Invalid payload" });

        var updated = await _activitiesService.ResubmitActivityAsync(
            completionId,
            dto.UserId,
            dto.Notes,
            dto.Image // can be null (user only edits notes)
        );

        if (!updated.Success)
            return BadRequest(new { message = updated.Message });

        // return refreshed payload for the row
        var c = updated.Completion!;
        var result = new
        {
            id = c.Id,
            activityId = c.ActivityId,
            title = c.Activity?.Title ?? "Unknown",
            completedAt = c.CompletedAt,
            reviewStatus = c.ReviewStatus,
            adminNotes = c.AdminNotes,
            notes = c.Notes,
            quantity = c.Quantity,
            imageUrl = !string.IsNullOrWhiteSpace(c.ImagePath)
                ? $"{Request.Scheme}://{Request.Host}/uploads/{c.ImagePath}"
                : null
        };

        return Ok(new { message = "Activity resubmitted for review", completion = result });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error resubmitting completion {CompletionId}", completionId);
        return StatusCode(500, new { message = "An error occurred while resubmitting", error = ex.Message });
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