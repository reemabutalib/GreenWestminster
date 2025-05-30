namespace Server.Models;
public class ActivityCompletion
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ActivityId { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public int PointsEarned { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual SustainableActivity Activity { get; set; } = null!;
}