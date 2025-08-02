using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class DeleteActivityDto
    {
        [Required]
        public int Id { get; set; }
    }
}