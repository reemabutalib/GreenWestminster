using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;


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
    public double? Quantity { get; set; }  // optional quantity in appropriate units
    public string? AdminNotes { get; set; }

    [Column("co2e_reduction")]
    public double? Co2eReduction { get; set; }
   

    // Navigation properties
    [JsonIgnore]
    public User User { get; set; }
    public SustainableActivity Activity { get; set; }
}