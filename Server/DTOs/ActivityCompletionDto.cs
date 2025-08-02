using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    // DTO for activity completion requests
    public class ActivityCompletionDto
    {
        [Required]
        public int UserId { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Make notes optional
        public string? Notes { get; set; } = null;

        // Make image optional
        public IFormFile? Image { get; set; } = null;
    }
}