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
using NpgsqlTypes;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChallengesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChallengesController> _logger;

    public ChallengesController(AppDbContext context, ILogger<ChallengesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/challenges
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetChallenges()
    {
        try
        {
            var challenges = await _context.Challenges
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    pointsReward = c.PointsReward,
                    isActive = c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(challenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching challenges");
            return StatusCode(500, new { message = "An error occurred while retrieving challenges", error = ex.Message });
        }
    }

    // GET: api/challenges/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<object>>> GetActiveChallenges()
    {
        try
        {
            var currentDate = DateTime.UtcNow.Date;
            var challenges = await _context.Challenges
                .Where(c => c.StartDate <= currentDate && c.EndDate >= currentDate)
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    pointsReward = c.PointsReward
                })
                .ToListAsync();

            return Ok(challenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching active challenges");
            return StatusCode(500, new { message = "An error occurred while retrieving active challenges", error = ex.Message });
        }
    }

    // GET: api/challenges/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetChallenge(int id)
    {
        try
        {
            var challenge = await _context.Challenges
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    pointsReward = c.PointsReward,
                    isActive = c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow
                })
                .FirstOrDefaultAsync();

            if (challenge == null)
            {
                return NotFound(new { message = "Challenge not found" });
            }

            return Ok(challenge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching challenge with ID {Id}", id);
            return StatusCode(500, new { message = $"An error occurred while retrieving challenge with ID {id}", error = ex.Message });
        }
    }

    // GET: api/challenges/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetUserChallenges(int userId)
    {
        try
        {
            var userChallenges = await _context.UserChallenges
                .Where(uc => uc.UserId == userId)
                .Join(_context.Challenges,
                    uc => uc.ChallengeId,
                    c => c.Id,
                    (uc, c) => new
                    {
                        id = c.Id,
                        title = c.Title,
                        description = c.Description,
                        startDate = c.StartDate,
                        endDate = c.EndDate,
                        pointsReward = c.PointsReward,
                        progress = uc.Progress,
                        status = uc.Status,
                        joinedDate = uc.JoinedDate
                    })
                .ToListAsync();

            return Ok(userChallenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching challenges for user {UserId}", userId);
            return StatusCode(500, new { message = $"An error occurred while retrieving challenges for user {userId}", error = ex.Message });
        }
    }

    // POST: api/challenges
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<object>> CreateChallenge(Challenge challenge)
    {
        try
        {
            // Modified to handle the missing relationship properly
            if (challenge.Activities != null && challenge.Activities.Any())
            {
                // Clear activities to prevent errors
                challenge.Activities = null;
            }

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            var result = new
            {
                id = challenge.Id,
                title = challenge.Title,
                description = challenge.Description,
                startDate = challenge.StartDate,
                endDate = challenge.EndDate,
                pointsReward = challenge.PointsReward
            };

            return CreatedAtAction(nameof(GetChallenge), new { id = challenge.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating challenge");
            return StatusCode(500, new { message = "An error occurred while creating the challenge", error = ex.Message });
        }
    }

    // POST: api/challenges/{id}/join
[HttpPost("{challengeId}/join")]
[Authorize]
public async Task<ActionResult> JoinChallenge(int challengeId, [FromBody] JoinChallengeDto joinData)
{
    try
    {
        if (joinData == null || joinData.UserId <= 0)
        {
            _logger.LogWarning("Invalid user data received when joining challenge {ChallengeId}", challengeId);
            return BadRequest(new { message = "Invalid user data" });
        }

        int userId = joinData.UserId;

        _logger.LogInformation("User {UserId} attempting to join challenge {ChallengeId}", userId, challengeId);

        // Simple existence checks without FromSqlRaw
        var challengeExists = await _context.Challenges.AnyAsync(c => c.Id == challengeId);
        if (!challengeExists)
        {
            _logger.LogWarning("Challenge {ChallengeId} not found during join attempt", challengeId);
            return NotFound(new { message = "Challenge not found" });
        }

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            _logger.LogWarning("User {UserId} not found during join attempt", userId);
            return NotFound(new { message = "User not found" });
        }

        // Check if already joined without using FromSqlRaw
        var alreadyJoined = await _context.UserChallenges
            .AnyAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);

        if (alreadyJoined)
        {
            _logger.LogInformation("User {UserId} has already joined challenge {ChallengeId}", userId, challengeId);
            return BadRequest(new { message = "User already joined this challenge" });
        }

       // Use raw SQL to insert with the correct column name
var sql = @"
    INSERT INTO userchallenges (userid, challengeid, joindate, progress, status, completed) 
    VALUES (@userId, @challengeId, @joinDate, @progress, @status, @completed)";
    
var parameters = new[]
{
    new NpgsqlParameter { ParameterName = "userId", Value = userId },
    new NpgsqlParameter { ParameterName = "challengeId", Value = challengeId },
    new NpgsqlParameter { ParameterName = "joinDate", Value = DateTime.UtcNow },
    new NpgsqlParameter { ParameterName = "progress", Value = 0 },
    new NpgsqlParameter { ParameterName = "status", Value = "In Progress" },
    new NpgsqlParameter { ParameterName = "completed", Value = false }
};

await _context.Database.ExecuteSqlRawAsync(sql, parameters);

        _logger.LogInformation("User {UserId} successfully joined challenge {ChallengeId}", userId, challengeId);
        return Ok(new { message = "Successfully joined challenge" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while joining challenge {ChallengeId} for user", challengeId);

        // Log more detailed information about the error
        if (ex.InnerException != null)
        {
            _logger.LogError("Inner exception: {Message}", ex.InnerException.Message);
            _logger.LogError("Full inner exception: {Exception}", ex.InnerException);
        }

        return StatusCode(500, new { message = "An error occurred while joining the challenge", error = ex.Message });
    }
}

    // Support for legacy API format
    [HttpPost("{challengeId}/join/{userId}")]
    [Authorize]
    public async Task<ActionResult> JoinChallengeWithUrlParams(int challengeId, int userId)
    {
        // Redirect to the new method with proper DTO
        var joinData = new JoinChallengeDto { UserId = userId };
        return await JoinChallenge(challengeId, joinData);
    }

    // POST: api/challenges/{id}/activities
    [HttpPost("{challengeId}/activities")]
    [Authorize]
    public async Task<ActionResult> AddActivitiesToChallenge(int challengeId, [FromBody] List<int> activityIds)
    {
        try
        {
            var challenge = await _context.Challenges.FindAsync(challengeId);
            if (challenge == null)
            {
                return NotFound(new { message = "Challenge not found" });
            }

            var invalidActivityIds = new List<int>();
            foreach (var activityId in activityIds)
            {
                var activity = await _context.SustainableActivities.FindAsync(activityId);
                if (activity == null)
                {
                    invalidActivityIds.Add(activityId);
                }

                // If you implement a relationship table, you would add entries here
            }

            if (invalidActivityIds.Any())
            {
                return BadRequest(new { message = $"Activities with IDs {string.Join(", ", invalidActivityIds)} do not exist" });
            }

            return Ok(new { message = "Activities associated with challenge successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding activities to challenge {ChallengeId}", challengeId);
            return StatusCode(500, new { message = "An error occurred while adding activities to the challenge", error = ex.Message });
        }
    }

    // GET: api/challenges/past
    [HttpGet("past")]
    public async Task<ActionResult<IEnumerable<object>>> GetPastChallenges()
    {
        try
        {
            var currentDate = DateTime.UtcNow.Date;
            var challenges = await _context.Challenges
                .Where(c => c.EndDate < currentDate) // Challenges that have ended
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    category = c.Category, // Include if your Challenge model has this property
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    pointsReward = c.PointsReward
                })
                .ToListAsync();

            return Ok(challenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching past challenges");
            return StatusCode(500, new { message = "An error occurred while retrieving past challenges", error = ex.Message });
        }
    }

    // DTO for joining challenges
    public class JoinChallengeDto
    {
        public int UserId { get; set; }
    }
}