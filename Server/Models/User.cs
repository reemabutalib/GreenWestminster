namespace Server.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Store hashed password
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public int Points { get; set; } = 0;
    public int CurrentStreak { get; set; } = 0;
    public int MaxStreak { get; set; } = 0;
    public DateTime LastActivityDate { get; set; } = DateTime.UtcNow;
    public virtual ICollection<ActivityCompletion> CompletedActivities { get; set; } = new List<ActivityCompletion>();
    public virtual ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
}