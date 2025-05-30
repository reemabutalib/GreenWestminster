using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

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
        return await _context.Challenges
            .Include(c => c.Activities)
            .ToListAsync();
    }

    // GET: api/challenges/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Challenge>>> GetActiveChallenges()
    {
        var currentDate = DateTime.UtcNow.Date;
        return await _context.Challenges
            .Include(c => c.Activities)
            .Where(c => c.StartDate <= currentDate && c.EndDate >= currentDate)
            .ToListAsync();
    }

    // GET: api/challenges/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Challenge>> GetChallenge(int id)
    {
        var challenge = await _context.Challenges
            .Include(c => c.Activities)
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
        // Validate activities
        if (challenge.Activities != null && challenge.Activities.Any())
        {
            List<SustainableActivity> activitiesToLink = new();
            
            foreach (var activity in challenge.Activities)
            {
                var existingActivity = await _context.SustainableActivities.FindAsync(activity.Id);
                if (existingActivity == null)
                {
                    return BadRequest($"Activity with ID {activity.Id} does not exist");
                }
                activitiesToLink.Add(existingActivity);
            }
            
            challenge.Activities = activitiesToLink;
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
            ChallengeId = challengeId
        };
        
        _context.UserChallenges.Add(userChallenge);
        await _context.SaveChangesAsync();
        
        return Ok();
    }
}