using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    // DTO for creating a new activity
    // Only include the properties that exist in your database
    public class CreateActivityDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int PointsValue { get; set; }
        [Required]
        public string Category { get; set; } = string.Empty;
        [Required]
        public bool IsDaily { get; set; }
        [Required]
        public bool IsWeekly { get; set; }
        [Required]
        public bool IsOneTime { get; set; }
        // Only include the properties that exist in your database
    }
}