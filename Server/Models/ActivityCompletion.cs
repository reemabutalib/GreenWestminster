namespace Server.Models;
public class ActivityCompletion
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ActivityId { get; set; }
    public DateTime CompletedAt { get; set; } // Changed from CompletionDate to CompletedAt
    public int PointsEarned { get; set; }
    
    // Navigation properties
    public User User { get; set; }
    public SustainableActivity Activity { get; set; }
}