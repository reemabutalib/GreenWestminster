namespace Server.Models;
public class UserChallenge
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ChallengeId { get; set; }
    public bool Completed { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Challenge Challenge { get; set; } = null!;
}