using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    // DTO for joining a challenge
    // Contains only the UserId as required field
    // This is used when a user wants to join a challenge   
    public class JoinChallengeDto
    {
        [Required]
        public int UserId { get; set; }
    }
}