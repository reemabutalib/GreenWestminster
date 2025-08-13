using System;

namespace Server.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime Created { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; }
    }
}
