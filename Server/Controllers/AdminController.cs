using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Server.Services.Interfaces;
using Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        private readonly AppDbContext _context;

        public AdminController(
        IAdminService adminService,
        ILogger<AdminController> logger,
        AppDbContext context) // <-- Add this
        {
            _adminService = adminService;
            _logger = logger;
            _context = context; // <-- Assign it
        }


        // GET: api/admin/user-stats
        [HttpGet("user-stats")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<object>> GetUserStats()
        {
            try
            {
                _logger.LogInformation("Admin accessing user engagement statistics");

                var stats = await _adminService.GetUserStatsAsync();

                // Filter out sustainabilityteam from user stats if the service returns per-user data
                if (stats is IEnumerable<User> userList)
                {
                    stats = userList.Where(u => u.Username != "sustainabilityteam");
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user engagement statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving user statistics", error = ex.Message });
            }
        }

        // GET: api/admin/activity-completions
        [HttpGet("activity-completions")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> GetActivityCompletions(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? reviewStatus = null)
        {
            try
            {
                _logger.LogInformation("Admin fetching activity completions");

                var baseUrl = (Request?.Scheme != null && Request?.Host.HasValue == true)
                    ? $"{Request.Scheme}://{Request.Host}"
                    : "http://localhost:5138";

                var query = _context.ActivityCompletions
                    .Include(ac => ac.User)
                    .Include(ac => ac.Activity)
                    .AsNoTracking()
                    .AsQueryable();

                // null-safe filter: exclude sustainabilityteam only when User exists
                query = query.Where(ac => ac.User == null || ac.User.Username != "sustainabilityteam");

                if (!string.IsNullOrWhiteSpace(reviewStatus))
                    query = query.Where(ac => ac.ReviewStatus == reviewStatus);

                if (startDate.HasValue)
                    query = query.Where(ac => ac.CompletedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(ac => ac.CompletedAt <= endDate.Value);

                var completions = await query
                    .OrderByDescending(ac => ac.CompletedAt)
                    .Select(ac => new
                    {
                        id = ac.Id,
                        userId = ac.UserId,
                        activityId = ac.ActivityId,
                        username = ac.User != null ? ac.User.Username : "Unknown",
                        activityTitle = ac.Activity != null ? ac.Activity.Title : "Unknown",
                        completedAt = ac.CompletedAt,
                        imagePath = ac.ImagePath,
                        imageUrl = !string.IsNullOrWhiteSpace(ac.ImagePath)
                            ? $"{baseUrl}/uploads/{ac.ImagePath.Trim()}"
                            : null,
                        notes = ac.Notes,
                        pointsEarned = ac.PointsEarned,
                        reviewStatus = ac.ReviewStatus,
                        adminNotes = ac.AdminNotes
                    })
                    .ToListAsync();

                return Ok(completions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching activity completions");
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving activity completions.",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }

        }

    }
}
