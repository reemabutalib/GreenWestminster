public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public int Points { get; set; }
    public string? AvatarStyle { get; set; }
    public string Level { get; set; }
    public int CurrentStreak { get; set; }
    public int MaxStreak { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public string Course { get; set; }
    public int YearOfStudy { get; set; }
    public string AccommodationType { get; set; }
    public DateTime JoinDate { get; set; }
}
