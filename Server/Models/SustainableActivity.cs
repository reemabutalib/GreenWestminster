namespace Server.Models;
public class SustainableActivity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsValue { get; set; }
    public string Category { get; set; } = string.Empty; // e.g., Waste Reduction, Sustainable Transport, Ethical Consumption
    public bool IsDaily { get; set; } = false;
    public bool IsWeekly { get; set; } = false;
    public bool IsOneTime { get; set; } = false;
    public virtual ICollection<ActivityCompletion> Completions { get; set; } = new List<ActivityCompletion>();
}