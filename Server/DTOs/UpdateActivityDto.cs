using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    // DTO for updating an activity
    // This DTO is used when an admin updates the details of an existing activity   
    public class UpdateActivityDto
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public int PointsValue { get; set; }

        [Required]
        public bool IsDaily { get; set; }

        [Required]
        public bool IsWeekly { get; set; }

        [Required]
        public bool IsOneTime { get; set; }
    }
}