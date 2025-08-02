using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    // DTO for user login requests
    // This DTO is used when a user attempts to log in to the application
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}