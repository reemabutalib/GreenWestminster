using Server.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Services.Interfaces;
using Server.Models;
using Server.DTOs;
using Server.Repositories;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _userService.GetUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
public async Task<ActionResult<UserDto>> GetUser(int id)
{
    var user = await _userService.GetUserAsync(id);
    if (user == null)
        return NotFound();

    var dto = new UserDto
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Points = user.Points,
        AvatarStyle = user.AvatarStyle,
        Level = user.Level,
        CurrentStreak = user.CurrentStreak,
        MaxStreak = user.MaxStreak,
        LastActivityDate = user.LastActivityDate,
        Course = user.Course,
        YearOfStudy = user.YearOfStudy,
        AccommodationType = user.AccommodationType
    };

    return Ok(dto);
}


    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        var createdUser = await _userService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
    }

    // GET: api/users/leaderboard
    [HttpGet("leaderboard")]
    public async Task<ActionResult<IEnumerable<LeaderboardUserDto>>> GetLeaderboard([FromQuery] string timeFrame = "all-time")
    {
        var leaderboard = await _userService.GetLeaderboardAsync(timeFrame);
        return Ok(leaderboard);
    }

    // GET: api/users/5/stats
    [HttpGet("{id}/stats")]
    public async Task<ActionResult<object>> GetUserStats(int id)
    {
        var stats = await _userService.GetUserStatsAsync(id);
        if (stats == null)
            return NotFound();
        return Ok(stats);
    }

    // GET: api/users/5/activities/recent
    [HttpGet("{id}/activities/recent")]
    public async Task<ActionResult<IEnumerable<object>>> GetRecentActivities(int id)
    {
        var activities = await _userService.GetRecentActivitiesAsync(id);
        if (activities == null)
            return NotFound();
        return Ok(activities);
    }

    // GET: api/users/5/activities/completed
    [HttpGet("{id}/activities/completed")]
    public async Task<ActionResult<IEnumerable<object>>> GetCompletedActivities(int id, [FromQuery] DateTime date)
    {
        var activities = await _userService.GetCompletedActivitiesAsync(id, date);
        if (activities == null)
            return NotFound();
        return Ok(activities);
    }

    // GET: api/users/5/challenges/completed
    [HttpGet("{id}/challenges/completed")]
    public async Task<ActionResult<IEnumerable<object>>> GetCompletedChallenges(int id)
    {
        var challenges = await _userService.GetCompletedChallengesAsync(id);
        if (challenges == null)
            return NotFound();
        return Ok(challenges);
    }

    // GET: api/users/5/challenges/1/status
    [HttpGet("{userId}/challenges/{challengeId}/status")]
    public async Task<ActionResult<object>> GetChallengeStatus(int userId, int challengeId)
    {
        var status = await _userService.GetChallengeStatusAsync(userId, challengeId);
        if (status == null)
            return NotFound();
        return Ok(status);
    }

    // POST: api/users/5/completeActivity/3
    [HttpPost("{userId}/completeActivity/{activityId}")]
    public async Task<ActionResult<ActivityCompletion>> CompleteActivity(int userId, int activityId)
    {
        var completion = await _userService.CompleteActivityAsync(userId, activityId);
        if (completion == null)
            return BadRequest("Activity already completed today or user/activity not found");
        return CreatedAtAction(nameof(GetUser), new { id = userId }, completion);
    }

    // GET: api/users/{userId}/activity-stats
    [HttpGet("{userId}/activity-stats")]
    public async Task<ActionResult<object>> GetActivityStats(int userId)
    {
        var stats = await _userService.GetActivityStatsAsync(userId);
        if (stats == null)
            return NotFound("User not found");
        return Ok(stats);
    }

    // GET: api/users/{userId}/points-history
    [HttpGet("{userId}/points-history")]
    public async Task<ActionResult<IEnumerable<object>>> GetPointsHistory(int userId)
    {
        var history = await _userService.GetPointsHistoryAsync(userId);
        if (history == null)
            return NotFound("User not found");
        return Ok(history);
    }

    // POST: api/users/{userId}/add-points
    [HttpPost("{userId}/add-points")]
    public async Task<IActionResult> AddPoints(int userId, [FromBody] AddPointsRequest request)
    {
        var result = await _userService.AddPointsAsync(userId, request.Points);
        return Ok(result);
    }

    // GET: api/users/{userId}/level-info
    [HttpGet("{userId}/level-info")]
    public async Task<ActionResult<object>> GetLevelInfo(int userId)
    {
        var info = await _userService.GetLevelInfoAsync(userId);
        if (info == null)
            return NotFound("User not found");
        return Ok(info);
    }

    // POST: api/users/avatar
    [HttpPost("avatar")]
public async Task<IActionResult> UpdateAvatar([FromBody] AvatarUpdateDto dto)
{
    var result = await _userService.UpdateAvatarAsync(dto.UserId, dto.AvatarStyle);
    if (!result) return NotFound();
    return Ok();
}
   
}

