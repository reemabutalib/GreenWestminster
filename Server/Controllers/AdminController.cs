using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Server.Services.Interfaces;
using Server.Data;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        private readonly AppDbContext _context;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
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
        public async Task<ActionResult<object>> GetActivityCompletions([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Admin accessing activity completion data");
                var completions = await _adminService.GetActivityCompletionsAsync(startDate, endDate);
                return Ok(completions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching activity completion data");
                return StatusCode(500, new { message = "An error occurred while retrieving activity completion data", error = ex.Message });
            }
        }
    }
}