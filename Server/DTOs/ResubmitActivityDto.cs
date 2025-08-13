using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Server.DTOs
{
    public class ResubmitActivityDto
    {
        [Required]
        public int UserId { get; set; }

        // Optional new notes
        public string? Notes { get; set; }

        // Optional new image (if the user wants to replace the evidence)
        public IFormFile? Image { get; set; }
    }
}
