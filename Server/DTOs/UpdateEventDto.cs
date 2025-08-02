using Server.Controllers;

namespace Server.DTOs
{
    // DTO for updating an event
    // This DTO is used when an admin updates the details of an existing event
    public class UpdateEventDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? RegistrationLink { get; set; }
        public IFormFile? Image { get; set; }
        public string? Organizer { get; set; }
        public int? MaxAttendees { get; set; }
        public string? Category { get; set; }
        public bool IsVirtual { get; set; }
    }
}
