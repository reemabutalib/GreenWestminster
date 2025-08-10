namespace Server.DTOs
{
    public class LeaderboardUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Points { get; set; }
        public int CurrentStreak { get; set; }
    }
}
