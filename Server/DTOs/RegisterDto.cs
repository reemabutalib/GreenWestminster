using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    // DTO for user registration requests
    // This DTO is used when a new user registers to the application    
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Course { get; set; } = string.Empty;

        [Required]
        [Range(1, 6)]
        public int YearOfStudy { get; set; } = 1;

        [Required]
        public string AccommodationType { get; set; } = string.Empty;
    }
}