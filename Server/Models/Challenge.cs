namespace Server.Models;
public class Challenge
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PointsReward { get; set; }
    public string Category { get; set; } = string.Empty;
    public virtual ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
    public virtual ICollection<SustainableActivity> Activities { get; set; } = new List<SustainableActivity>();
}