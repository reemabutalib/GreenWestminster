using System;

namespace Server.Models
{
    public class SustainabilityEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RegistrationLink { get; set; }
        public string ImageUrl { get; set; }
        public string Organizer { get; set; }
        public int? MaxAttendees { get; set; }
        public string Category { get; set; }
        public bool IsVirtual { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}