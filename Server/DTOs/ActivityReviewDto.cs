using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{

    // DTO for activity review requests
    public class ActivityReviewDto
    {
        [Required]
        public string Status { get; set; } = "Approved"; // Approved, Rejected, Pending Review
        public string? AdminNotes { get; set; }
    }
}