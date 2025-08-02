using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class CreateEventDto
    {
        [Required]
        public string Title { get; set; }

    [Required]
    public string Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? RegistrationLink { get; set; }
    public IFormFile? Image { get; set; }
    public string? Organizer { get; set; }
    public int? MaxAttendees { get; set; }
    public string? Category { get; set; }
    public bool IsVirtual { get; set; } = false;
}

}