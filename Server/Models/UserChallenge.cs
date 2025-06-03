namespace Server.Models;
public class UserChallenge
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ChallengeId { get; set; }
    
    // Your existing property for tracking completion
    public bool Completed { get; set; } = false;
    
    // Adding alias for compatibility with controller code
    public bool IsCompleted { get => Completed; set => Completed = value; }
    
    public DateTime? CompletedAt { get; set; }
    
    // Add a CompletedDate property that maps to CompletedAt
    public DateTime? CompletedDate { get => CompletedAt; set => CompletedAt = value; }
    
    // Add tracking for user's progress in the challenge (0-100%)
    public int Progress { get; set; } = 0;
    
    // Add property to track when user joined the challenge
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    
    // Add property to track challenge status as a string
    public string Status { get; set; } = "Completed";

    public virtual User User { get; set; } = null!;
    public virtual Challenge Challenge { get; set; } = null!;
}