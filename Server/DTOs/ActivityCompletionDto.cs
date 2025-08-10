using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Server.DTOs
{
    public class ActivityCompletionDto
    {
        [Required]
        public int UserId { get; set; }

        public DateTime? CompletedAt { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public double? Quantity { get; set; }

        [Required(ErrorMessage = "Image evidence is required")]
        public IFormFile Image { get; set; }  // ← no longer nullable

        public string? Notes { get; set; } = null;
    }
}
