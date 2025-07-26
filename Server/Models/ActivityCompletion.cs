namespace Server.Models;
public class ActivityCompletion
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ActivityId { get; set; }
    public DateTime CompletedAt { get; set; }
    public int PointsEarned { get; set; }
    public string ImagePath { get; set; }
    public string Notes { get; set; }
    public string ReviewStatus { get; set; } = "Pending Review";
    
    // Navigation properties
    public User User { get; set; }
    public SustainableActivity Activity { get; set; }
}