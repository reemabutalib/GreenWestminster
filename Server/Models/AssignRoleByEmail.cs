namespace Server.Models
{
    // Model for assigning a role to a user by email

    public class AssignRoleByEmailModel
    {
        public string Email { get; set; }
        public string RoleName { get; set; }
    }
}