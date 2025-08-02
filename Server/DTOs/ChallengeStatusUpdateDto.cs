using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    // DTO for updating challenge status
    public class ChallengeStatusUpdateDto
    {
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? EndNow { get; set; }
    }
}