namespace Server.Models
{
    // Model for creating a new identity user

    public class CreateIdentityUserModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}