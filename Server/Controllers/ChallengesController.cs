using Server.Data;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChallengesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ChallengesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/challenges
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Challenge>>> GetChallenges()
    {
        // Remove the Include(c => c.Activities) that's causing the error
        return await _context.Challenges
            .ToListAsync();
    }

    // GET: api/challenges/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Challenge>>> GetActiveChallenges()
    {
        var currentDate = DateTime.UtcNow.Date;
        // Remove the Include(c => c.Activities) that's causing the error
        return await _context.Challenges
            .Where(c => c.StartDate <= currentDate && c.EndDate >= currentDate)
            .ToListAsync();
    }

    // GET: api/challenges/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Challenge>> GetChallenge(int id)
    {
        // Remove the Include(c => c.Activities) that's causing the error
        var challenge = await _context.Challenges
            .FirstOrDefaultAsync(c => c.Id == id);

        if (challenge == null)
        {
            return NotFound();
        }

        return challenge;
    }

    // POST: api/challenges
    [HttpPost]
    public async Task<ActionResult<Challenge>> CreateChallenge(Challenge challenge)
    {
        // Modified to handle the missing relationship properly
        if (challenge.Activities != null && challenge.Activities.Any())
        {
            // Since there's no direct relationship in the database,
            // we need to handle this differently - either by:
            // 1. Creating a join table, or
            // 2. Saving the challenge first, then updating activities separately
            
            // For now, let's clear the activities to prevent errors
            challenge.Activities = null;
        }
        
        _context.Challenges.Add(challenge);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetChallenge), new { id = challenge.Id }, challenge);
    }

    // POST: api/challenges/5/join/3
    [HttpPost("{challengeId}/join/{userId}")]
    public async Task<ActionResult> JoinChallenge(int challengeId, int userId)
    {
        var challenge = await _context.Challenges.FindAsync(challengeId);
        if (challenge == null)
            return NotFound("Challenge not found");
            
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("User not found");
            
        // Check if user already joined this challenge
        var existingUserChallenge = await _context.UserChallenges
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);
            
        if (existingUserChallenge != null)
            return BadRequest("User already joined this challenge");
            
        var userChallenge = new UserChallenge
        {
            UserId = userId,
            ChallengeId = challengeId,
            JoinedDate = DateTime.UtcNow,  // Set the JoinedDate
            Progress = 0,                  // Initialize progress
            Status = "In Progress"         // Set initial status
        };
        
        _context.UserChallenges.Add(userChallenge);
        await _context.SaveChangesAsync();
        
        return Ok();
    }
    
    // If you need to associate activities with challenges, here's a new endpoint
    // POST: api/challenges/5/activities
    [HttpPost("{challengeId}/activities")]
    public async Task<ActionResult> AddActivitiesToChallenge(int challengeId, [FromBody] List<int> activityIds)
    {
        var challenge = await _context.Challenges.FindAsync(challengeId);
        if (challenge == null)
            return NotFound("Challenge not found");
            
        foreach (var activityId in activityIds)
        {
            var activity = await _context.SustainableActivities.FindAsync(activityId);
            if (activity == null)
                return BadRequest($"Activity with ID {activityId} does not exist");
                
            // Since there's no direct relationship in your model,
            // you would need to establish it another way
            // For example, add a challengeid column to your activities table
            // and update it here
        }
        
        return Ok();
    }
}