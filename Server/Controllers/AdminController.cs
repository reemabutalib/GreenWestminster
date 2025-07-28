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

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
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

                // User counts
                int totalUsers = await _context.Users.CountAsync();
                int activeUsersLast7Days = await _context.Users.CountAsync(u => u.LastActivityDate >= DateTime.UtcNow.AddDays(-7));
                int activeUsersLast30Days = await _context.Users.CountAsync(u => u.LastActivityDate >= DateTime.UtcNow.AddDays(-30));
                
                // Top users by points
                var topUsers = await _context.Users
                    .OrderByDescending(u => u.Points)
                    .Take(10)
                    .Select(u => new { 
                        id = u.Id,
                        username = u.Username,
                        points = u.Points,
                        currentStreak = u.CurrentStreak,
                        maxStreak = u.MaxStreak,
                        joinDate = u.JoinDate
                    })
                    .ToListAsync();

                // Activities statistics
                int totalActivities = await _context.SustainableActivities.CountAsync();
                int totalCompletions = await _context.ActivityCompletions.CountAsync();
                
                // Most popular activities (by completions)
                var popularActivities = await _context.ActivityCompletions
                    .GroupBy(ac => ac.ActivityId)
                    .Select(g => new { 
                        activityId = g.Key, 
                        completionCount = g.Count() 
                    })
                    .OrderByDescending(a => a.completionCount)
                    .Take(5)
                    .ToListAsync();

                // Get activity details for the popular activities
                var activityIds = popularActivities.Select(a => a.activityId).ToList();
                var activityDetails = await _context.SustainableActivities
                    .Where(a => activityIds.Contains(a.Id))
                    .Select(a => new { 
                        id = a.Id, 
                        title = a.Title, 
                        category = a.Category 
                    })
                    .ToDictionaryAsync(a => a.id, a => new { a.title, a.category });
                
                var popularActivitiesWithDetails = popularActivities
                    .Select(a => new { 
                        activityId = a.activityId,
                        title = activityDetails.ContainsKey(a.activityId) ? activityDetails[a.activityId].title : "Unknown Activity",
                        category = activityDetails.ContainsKey(a.activityId) ? activityDetails[a.activityId].category : "Unknown",
                        completionCount = a.completionCount
                    })
                    .ToList();

                // Events statistics
                int totalEvents = await _context.SustainabilityEvents.CountAsync();
                int upcomingEvents = await _context.SustainabilityEvents.CountAsync(e => e.StartDate > DateTime.UtcNow);

                // Get points distribution
                var pointsDistributionQuery = await _context.Users
                .GroupBy(u => u.Points / 100) // Group users by 100-point ranges
                .Select(g => new
                {
                pointRange = g.Key,  // Just select the range key (will format client-side)
                userCount = g.Count()
            })
                .OrderBy(g => g.pointRange)
                 .ToListAsync();

                 var pointsDistribution = pointsDistributionQuery
    .Select(g => new {
        pointRange = $"{g.pointRange * 100}-{(g.pointRange + 1) * 100 - 1}",
        userCount = g.userCount
    })
    .ToList();

                // Activity completions by date (last 30 days)
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var dailyCompletions = await _context.ActivityCompletions
                    .Where(ac => ac.CompletedAt >= thirtyDaysAgo)
                    .GroupBy(ac => ac.CompletedAt.Date)
                    .Select(g => new { 
                        date = g.Key, 
                        count = g.Count() 
                    })
                    .OrderBy(d => d.date)
                    .ToListAsync();
                
                // Combine all statistics
                var stats = new
                {
                    users = new
                    {
                        totalCount = totalUsers,
                        activeLast7Days = activeUsersLast7Days,
                        activeLast30Days = activeUsersLast30Days,
                        topByPoints = topUsers,
                        pointsDistribution = pointsDistribution
                    },
                    activities = new
                    {
                        totalCount = totalActivities,
                        totalCompletions = totalCompletions,
                        popularActivities = popularActivitiesWithDetails,
                        dailyCompletions = dailyCompletions
                    },
                    events = new
                    {
                        totalCount = totalEvents,
                        upcomingCount = upcomingEvents
                    }
                };

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
                
                // Default to last 30 days if no date range provided
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                // Get all completions within date range with user and activity details
                var query = @"
                    SELECT 
                        ac.id, 
                        ac.userid, 
                        u.username, 
                        ac.activityid, 
                        sa.title as activitytitle, 
                        sa.category, 
                        sa.pointsvalue, 
                        ac.completedat, 
                        ac.status, 
                        ac.adminnotes
                    FROM 
                        activitycompletions ac
                    JOIN 
                        users u ON ac.userid = u.id
                    JOIN 
                        sustainableactivities sa ON ac.activityid = sa.id
                    WHERE 
                        ac.completedat BETWEEN @startDate AND @endDate
                    ORDER BY 
                        ac.completedat DESC";
                
                var parameters = new[]
                {
                    new NpgsqlParameter("startDate", start),
                    new NpgsqlParameter("endDate", end)
                };

                var completions = new List<object>();

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
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
                            completions.Add(new
                            {
                                id = reader.GetInt32(0),
                                userId = reader.GetInt32(1),
                                username = reader.GetString(2),
                                activityId = reader.GetInt32(3),
                                activityTitle = reader.GetString(4),
                                category = reader.GetString(5),
                                pointsValue = reader.GetInt32(6),
                                completedAt = reader.GetDateTime(7),
                                status = reader.IsDBNull(8) ? "Pending" : reader.GetString(8),
                                adminNotes = reader.IsDBNull(9) ? null : reader.GetString(9)
                            });
                        }
                    }
                }

                // Gather summary statistics
                var completionsByCategory = completions
                    .Cast<dynamic>()
                    .GroupBy(c => c.category)
                    .Select(g => new { 
                        category = g.Key, 
                        count = g.Count() 
                    })
                    .OrderByDescending(g => g.count)
                    .ToList();
                
                var completionsByStatus = completions
                    .Cast<dynamic>()
                    .GroupBy(c => c.status)
                    .Select(g => new { 
                        status = g.Key, 
                        count = g.Count() 
                    })
                    .OrderByDescending(g => g.count)
                    .ToList();

                return Ok(new
                {
                    completions,
                    summary = new
                    {
                        totalCount = completions.Count,
                        byCategory = completionsByCategory,
                        byStatus = completionsByStatus,
                        dateRange = new { start, end }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching activity completion data");
                return StatusCode(500, new { message = "An error occurred while retrieving activity completion data", error = ex.Message });
            }
        }
    }
}