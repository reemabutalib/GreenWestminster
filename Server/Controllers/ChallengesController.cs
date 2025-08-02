using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Server.Services.Interfaces;
using Server.DTOs;
using Server.Data;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChallengesController : ControllerBase
{
    private readonly IChallengesService _challengesService;
    private readonly ILogger<ChallengesController> _logger;
    private readonly AppDbContext _context;

    public ChallengesController(IChallengesService challengesService, ILogger<ChallengesController> logger)
    {
        _challengesService = challengesService;
        _logger = logger;
    }

    // GET: api/challenges
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetChallenges()
    {
        try
        {
            var challenges = await _challengesService.GetChallengesAsync();
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
            var challenges = await _challengesService.GetActiveChallengesAsync();
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
            var challenge = await _challengesService.GetChallengeByIdAsync(id);
            if (challenge == null)
                return NotFound(new { message = "Challenge not found" });

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
            var userChallenges = await _challengesService.GetUserChallengesAsync(userId);
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> CreateChallenge(Challenge challenge)
    {
        try
        {
            var result = await _challengesService.CreateChallengeAsync(challenge);
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
                return BadRequest(new { message = "Invalid user data" });

            var joined = await _challengesService.JoinChallengeAsync(challengeId, joinData.UserId);
            if (!joined)
                return BadRequest(new { message = "User already joined this challenge or challenge/user not found" });

            return Ok(new { message = "Successfully joined challenge" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while joining challenge {ChallengeId} for user", challengeId);
            return StatusCode(500, new { message = "An error occurred while joining the challenge", error = ex.Message });
        }
    }

    // Support for legacy API format
    [HttpPost("{challengeId}/join/{userId}")]
    [Authorize]
    public async Task<ActionResult> JoinChallengeWithUrlParams(int challengeId, int userId)
    {
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
            var added = await _challengesService.AddActivitiesToChallengeAsync(challengeId, activityIds);
            if (!added)
                return BadRequest(new { message = "Challenge or some activities not found" });

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
            var challenges = await _challengesService.GetPastChallengesAsync();
            return Ok(challenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching past challenges");
            return StatusCode(500, new { message = "An error occurred while retrieving past challenges", error = ex.Message });
        }
    }

    // PUT: api/challenges/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateChallenge(int id, Challenge challenge)
    {
        try
        {
            var updated = await _challengesService.UpdateChallengeAsync(id, challenge);
            if (updated == null)
                return NotFound(new { message = "Challenge not found or ID mismatch" });

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating challenge with ID {Id}", id);
            return StatusCode(500, new { message = $"An error occurred while updating the challenge", error = ex.Message });
        }
    }

    // DELETE: api/challenges/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteChallenge(int id)
    {
        try
        {
            var deleted = await _challengesService.DeleteChallengeAsync(id);
            if (!deleted)
                return NotFound(new { message = "Challenge not found" });

            return Ok(new { message = "Challenge deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting challenge with ID {Id}", id);
            return StatusCode(500, new { message = $"An error occurred while deleting the challenge", error = ex.Message });
        }
    }

    // PATCH: api/challenges/{id}/status
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateChallengeStatus(int id, [FromBody] ChallengeStatusUpdateDto statusUpdate)
    {
        try
        {
            var updated = await _challengesService.UpdateChallengeStatusAsync(id, statusUpdate);
            if (updated == null)
                return NotFound(new { message = "Challenge not found" });

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating status of challenge with ID {Id}", id);
            return StatusCode(500, new { message = $"An error occurred while updating the challenge status", error = ex.Message });
        }
    }
}